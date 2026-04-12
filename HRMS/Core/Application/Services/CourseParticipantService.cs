using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace HRMS.Core.Application.Services;

public class CourseParticipantService : ICourseParticipantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CourseParticipantService> _logger;
    private readonly IHostEnvironment _environment;
    private const string CertificatesFolder = "Certificates";
    private const decimal MinimumPassPercentage = 75.0m;

    public CourseParticipantService(
        IUnitOfWork unitOfWork,
        ILogger<CourseParticipantService> logger,
        IHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _environment = environment;
    }

    public async Task<CourseDetailWithParticipantsDto?> GetCourseWithParticipantsAsync(int coursePlanId, int tenantId)
    {
        try
        {
            var coursePlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(coursePlanId, tenantId);
            if (coursePlan == null)
                return null;

            var participants = await _unitOfWork.CourseParticipant.GetParticipantsByCoursePlanAsync(coursePlanId, tenantId);

            return new CourseDetailWithParticipantsDto
            {
                CoursePlanId = coursePlan.Id,
                CourseId = coursePlan.CourseId,
                CourseTitle = coursePlan.Course?.Title ?? string.Empty,
                CourseCode = coursePlan.Course?.Code,
                StartDate = coursePlan.StartDate,
                EndDate = coursePlan.EndDate,
                StartTime = coursePlan.StartTime,
                EndTime = coursePlan.EndTime,
                CourseDuration = coursePlan.Course?.Duration ?? 0,
                TrainerName = coursePlan.Trainer?.Name ?? string.Empty,
                Venue = coursePlan.Venue,
                TenantId = coursePlan.TenantId,
                Participants = participants.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course with participants for CoursePlanId: {CoursePlanId}", coursePlanId);
            return null;
        }
    }

    public async Task<bool> AddParticipantsAsync(AddCourseParticipantRequest request, int tenantId, int userId)
    {
        try
        {
            // Get course plan for calculating total days
            var coursePlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(request.CoursePlanId, tenantId);
            var totalDays = coursePlan != null ? (coursePlan.EndDate - coursePlan.StartDate).Days + 1 : 0;

            foreach (var staffId in request.StaffIds)
            {
                // Check if participant already exists (including inactive ones to avoid unique constraint violation)
                var existingParticipant = await _unitOfWork.CourseParticipant.GetParticipantAsync(request.CoursePlanId, staffId, tenantId);

                if (existingParticipant != null)
                {
                    if (existingParticipant.IsActive)
                    {
                        _logger.LogWarning("Participant already active: CoursePlanId={CoursePlanId}, StaffId={StaffId}", request.CoursePlanId, staffId);
                        continue;
                    }

                    // Reactivate inactive participant
                    existingParticipant.IsActive = true;
                    existingParticipant.UpdatedDate = DateTime.Now;
                    existingParticipant.UpdatedBy = userId;
                    await _unitOfWork.CourseParticipant.UpdateAsync(existingParticipant);
                    _logger.LogInformation("Reactivated participant: CoursePlanId={CoursePlanId}, StaffId={StaffId}", request.CoursePlanId, staffId);
                }
                else
                {
                    // Create new participant
                    var participant = new CourseParticipant
                    {
                        CoursePlanId = request.CoursePlanId,
                        StaffId = staffId,
                        TenantId = tenantId,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId
                    };

                    await _unitOfWork.CourseParticipant.AddAsync(participant);
                }

                // Check if result record exists and handle accordingly
                var existingResult = await _unitOfWork.CourseResult.GetByCoursePlanStaffAsync(request.CoursePlanId, staffId, tenantId);
                if (existingResult != null)
                {
                    if (!existingResult.IsActive)
                    {
                        existingResult.IsActive = true;
                        existingResult.UpdatedDate = DateTime.Now;
                        existingResult.UpdatedBy = userId;
                        await _unitOfWork.CourseResult.UpdateAsync(existingResult);
                    }
                }
                else
                {
                    // Create a result record for this participant
                    var result = new CourseResult
                    {
                        CoursePlanId = request.CoursePlanId,
                        StaffId = staffId,
                        TotalDays = totalDays,
                        PresentDays = 0,
                        AttendancePercentage = 0,
                        TenantId = tenantId,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId
                    };

                    await _unitOfWork.CourseResult.AddAsync(result);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participants to course: {CoursePlanId}", request.CoursePlanId);
            return false;
        }
    }

    public async Task<bool> RemoveParticipantAsync(int coursePlanId, int staffId, int tenantId, int userId)
    {
        try
        {
            var participants = await _unitOfWork.CourseParticipant.GetByCoursePlanIdAsync(coursePlanId, tenantId);
            var participant = participants.FirstOrDefault(p => p.StaffId == staffId);

            if (participant == null)
                return false;

            participant.IsActive = false;
            participant.UpdatedDate = DateTime.Now;
            participant.UpdatedBy = userId;

            await _unitOfWork.CourseParticipant.UpdateAsync(participant);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant: CoursePlanId={CoursePlanId}, StaffId={StaffId}", coursePlanId, staffId);
            return false;
        }
    }

    public async Task<IEnumerable<AttendanceGridDto>> GetAttendanceGridAsync(int coursePlanId, int tenantId)
    {
        try
        {
            var coursePlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(coursePlanId, tenantId);
            if (coursePlan == null)
                return Enumerable.Empty<AttendanceGridDto>();

            var participants  = await _unitOfWork.CourseParticipant.GetParticipantsByCoursePlanAsync(coursePlanId, tenantId);
            var attendances   = (await _unitOfWork.CourseAttendanceDateWise.GetAttendanceByCoursePlanAsync(coursePlanId, tenantId)).ToList();

            _logger.LogInformation(
                "AttendanceGrid: CoursePlanId={CoursePlanId} | participants={P} | attendance rows={A}",
                coursePlanId, participants.Count(), attendances.Count);

            foreach (var a in attendances)
                _logger.LogInformation(
                    "  Row: StaffId={StaffId} Date={Date:yyyy-MM-dd} IsPresent={IsPresent}",
                    a.StaffId, a.AttendanceDate, a.IsPresent);

            // Generate date range
            var dates = Enumerable.Range(0, (coursePlan.EndDate - coursePlan.StartDate).Days + 1)
                .Select(offset => coursePlan.StartDate.AddDays(offset))
                .ToList();

            var totalDays = dates.Count;

            var gridData = new List<AttendanceGridDto>();

            foreach (var participant in participants)
            {
                // Use "yyyy-MM-dd" string keys to avoid ALL DateTimeKind/timezone comparison
                // issues that can silently cause dictionary lookups to return false.
                var staffAttendances = attendances
                    .Where(a => a.StaffId == participant.StaffId)
                    .ToList();

                var attendanceByDate = dates.ToDictionary(
                    date => date.ToString("yyyy-MM-dd"),
                    date => staffAttendances.Any(a =>
                        a.AttendanceDate.ToString("yyyy-MM-dd") == date.ToString("yyyy-MM-dd") &&
                        a.IsPresent)
                );

                var presentDays = attendanceByDate.Count(kvp => kvp.Value);
                var attendancePercentage = totalDays > 0 ? (decimal)presentDays / totalDays * 100 : 0;

                gridData.Add(new AttendanceGridDto
                {
                    StaffId = participant.StaffId,
                    StaffName = participant.StaffName,
                    EmployeeCode = participant.EmployeeCode,
                    AttendanceByDate = attendanceByDate,
                    TotalDays = totalDays,
                    PresentDays = presentDays,
                    AttendancePercentage = Math.Round(attendancePercentage, 2)
                });
            }

            return gridData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attendance grid for CoursePlanId: {CoursePlanId}", coursePlanId);
            return Enumerable.Empty<AttendanceGridDto>();
        }
    }

    public async Task<bool> UpdateAttendanceAsync(UpdateAttendanceRequest request, int tenantId, int userId)
    {
        try
        {
            var existing = await _unitOfWork.CourseAttendanceDateWise.GetByCoursePlanStaffDateAsync(
                request.CoursePlanId,
                request.StaffId,
                request.AttendanceDate,
                tenantId);

            if (existing != null)
            {
                existing.IsPresent = request.IsPresent;
                existing.Remarks = request.Remarks;
                existing.UpdatedDate = DateTime.Now;
                existing.UpdatedBy = userId;
                await _unitOfWork.CourseAttendanceDateWise.UpdateAsync(existing);
            }
            else
            {
                var attendance = new CourseAttendanceDateWise
                {
                    CoursePlanId = request.CoursePlanId,
                    StaffId = request.StaffId,
                    AttendanceDate = request.AttendanceDate,
                    IsPresent = request.IsPresent,
                    Remarks = request.Remarks,
                    TenantId = tenantId,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };
                await _unitOfWork.CourseAttendanceDateWise.AddAsync(attendance);
            }

            await _unitOfWork.SaveChangesAsync();

            // Update result summary
            await UpdateResultSummaryAsync(request.CoursePlanId, request.StaffId, tenantId, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating attendance for CoursePlanId: {CoursePlanId}, StaffId: {StaffId}",
                request.CoursePlanId, request.StaffId);
            return false;
        }
    }

    private async Task UpdateResultSummaryAsync(int coursePlanId, int staffId, int tenantId, int userId)
    {
        try
        {
            var coursePlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(coursePlanId, tenantId);
            if (coursePlan == null)
                return;

            var totalDays = (coursePlan.EndDate - coursePlan.StartDate).Days + 1;

            var attendances = await _unitOfWork.CourseAttendanceDateWise.GetByCoursePlanIdAsync(coursePlanId, tenantId);
            var staffAttendances = attendances.Where(a => a.StaffId == staffId).ToList();

            var presentDays = staffAttendances.Count(a => a.IsPresent);
            var attendancePercentage = totalDays > 0 ? (decimal)presentDays / totalDays * 100 : 0;

            var result = await _unitOfWork.CourseResult.GetByCoursePlanStaffAsync(coursePlanId, staffId, tenantId);

            if (result != null)
            {
                result.TotalDays = totalDays;
                result.PresentDays = presentDays;
                result.AttendancePercentage = Math.Round(attendancePercentage, 2);
                result.UpdatedDate = DateTime.Now;
                result.UpdatedBy = userId;
                await _unitOfWork.CourseResult.UpdateAsync(result);
            }
            else
            {
                result = new CourseResult
                {
                    CoursePlanId = coursePlanId,
                    StaffId = staffId,
                    TotalDays = totalDays,
                    PresentDays = presentDays,
                    AttendancePercentage = Math.Round(attendancePercentage, 2),
                    TenantId = tenantId,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };
                await _unitOfWork.CourseResult.AddAsync(result);
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating result summary for CoursePlanId: {CoursePlanId}, StaffId: {StaffId}",
                coursePlanId, staffId);
        }
    }

    public async Task<IEnumerable<CourseResultDto>> GetResultSummaryAsync(int coursePlanId, int tenantId)
    {
        try
        {
            return await _unitOfWork.CourseResult.GetResultSummaryByCoursePlanAsync(coursePlanId, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting result summary for CoursePlanId: {CoursePlanId}", coursePlanId);
            return Enumerable.Empty<CourseResultDto>();
        }
    }

    public async Task<bool> UpdateResultAsync(UpdateResultRequest request, int tenantId, int userId)
    {
        try
        {
            var result = await _unitOfWork.CourseResult.GetByCoursePlanStaffAsync(request.CoursePlanId, request.StaffId, tenantId);
            if (result == null)
                return false;

            result.Marks = request.Marks;
            result.ResultStatus = request.ResultStatus;
            result.UpdatedDate = DateTime.Now;
            result.UpdatedBy = userId;

            await _unitOfWork.CourseResult.UpdateAsync(result);
            await _unitOfWork.SaveChangesAsync();

            // If Pass, generate certificate
            if (request.ResultStatus == "Pass")
            {
                await GenerateCertificateAsync(request.CoursePlanId, request.StaffId, tenantId, userId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating result for CoursePlanId: {CoursePlanId}, StaffId: {StaffId}",
                request.CoursePlanId, request.StaffId);
            return false;
        }
    }

    public async Task<bool> UpdateMarksAsync(int coursePlanId, int staffId, decimal? marks, int tenantId, int userId)
    {
        try
        {
            var result = await _unitOfWork.CourseResult.GetByCoursePlanStaffAsync(coursePlanId, staffId, tenantId);
            if (result == null)
                return false;

            result.Marks = marks;
            result.UpdatedDate = DateTime.Now;
            result.UpdatedBy = userId;

            await _unitOfWork.CourseResult.UpdateAsync(result);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating marks for CoursePlanId: {CoursePlanId}, StaffId: {StaffId}",
                coursePlanId, staffId);
            return false;
        }
    }

    public async Task<string?> GenerateCertificateAsync(int coursePlanId, int staffId, int tenantId, int userId)
    {
        try
        {
            var result = await _unitOfWork.CourseResult.GetByCoursePlanStaffAsync(coursePlanId, staffId, tenantId);
            if (result == null || result.ResultStatus != "Pass")
                return null;

            var coursePlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(coursePlanId, tenantId);
            if (coursePlan == null)
                return null;

            // Get staff details
            var staff = await _unitOfWork.Staff.GetByIdAsync(staffId);
            if (staff == null)
                return null;

            var courseCode = coursePlan.Course?.Code ?? "UNKNOWN";
            var courseTitle = coursePlan.Course?.Title ?? "Training Course";
            var staffName = staff.Name ?? "Participant";
            var staffCode = staff.EmployeeCode ?? staffId.ToString();
            var courseEndDate = coursePlan.EndDate;

            // Generate certificate serial number
            var serialNumber = $"{courseCode}-{tenantId:D3}-{result.ResultId:D5}";

            // Generate QR code content
            var qrContent = $"Certificate:{serialNumber}|Name:{staffName}|Course:{courseTitle}|Date:{coursePlan.StartDate:yyyy-MM-dd}";

            // Create certificates folder in wwwroot for static file serving
            var wwwrootPath = Path.Combine(_environment.ContentRootPath ?? "", "wwwroot");
            var certificateFolder = Path.Combine(wwwrootPath, CertificatesFolder, tenantId.ToString(), courseCode);
            Directory.CreateDirectory(certificateFolder);

            // Generate QR code image
            var qrCodeFileName = $"qr_{staffCode}_{result.ResultId}.png";
            var qrCodePath = Path.Combine(certificateFolder, qrCodeFileName);
            var qrCodeRelativePath = $"/{CertificatesFolder}/{tenantId}/{courseCode}/{qrCodeFileName}";
            using (var qrGenerator = new QRCoder.QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCoder.QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCoder.QRCode(qrCodeData))
            using (var qrCodeImage = qrCode.GetGraphic(10))
            {
                qrCodeImage.Save(qrCodePath, System.Drawing.Imaging.ImageFormat.Png);
            }

            // Certificate filename with date and staff name (sanitized)
            var sanitizedStaffName = SanitizeFileName(staffName);
            var certificateFileName = $"Certificate_{sanitizedStaffName}_{courseEndDate:yyyy-MM-dd}_{serialNumber}.html";
            var certificateFullPath = Path.Combine(certificateFolder, certificateFileName);

            // Generate HTML certificate
            var certificateHtml = GenerateCertificateHtml(
                staffName,
                staffCode,
                courseTitle,
                courseCode,
                coursePlan.StartDate,
                coursePlan.EndDate,
                serialNumber,
                qrCodeRelativePath,
                coursePlan.Trainer?.Name ?? "Trainer",
                result.Marks,
                result.AttendancePercentage
            );

            // Save HTML certificate file
            await File.WriteAllTextAsync(certificateFullPath, certificateHtml);

            // Store relative web path for database (starts with /)
            var relativeCertificatePath = $"/{CertificatesFolder}/{tenantId}/{courseCode}/{certificateFileName}";

            // Update result with relative certificate path and serial number
            result.CertificatePath = relativeCertificatePath;
            result.CertificateSerialNumber = serialNumber;
            result.UpdatedDate = DateTime.Now;
            result.UpdatedBy = userId;

            await _unitOfWork.CourseResult.UpdateAsync(result);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Certificate generated for CoursePlanId: {CoursePlanId}, StaffId: {StaffId}, Serial: {Serial}, Path: {Path}",
                coursePlanId, staffId, serialNumber, relativeCertificatePath);

            return relativeCertificatePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating certificate for CoursePlanId: {CoursePlanId}, StaffId: {StaffId}",
                coursePlanId, staffId);
            return null;
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Replace(" ", "_");
    }

    private static string GenerateCertificateHtml(
        string staffName,
        string staffCode,
        string courseTitle,
        string courseCode,
        DateTime startDate,
        DateTime endDate,
        string serialNumber,
        string qrCodePath,
        string trainerName,
        decimal? marks,
        decimal attendancePercentage)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Certificate of Completion - {staffName}</title>
    <style>
        @page {{
            size: A4 landscape;
            margin: 0;
        }}
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Georgia', 'Times New Roman', serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }}
        .certificate {{
            background: white;
            width: 1000px;
            min-height: 700px;
            padding: 40px;
            position: relative;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        .border-outer {{
            border: 3px solid #1a237e;
            padding: 20px;
            height: 100%;
        }}
        .border-inner {{
            border: 1px solid #1a237e;
            padding: 30px;
            text-align: center;
            min-height: 580px;
            display: flex;
            flex-direction: column;
            justify-content: space-between;
        }}
        .header {{
            margin-bottom: 20px;
        }}
        .logo {{
            font-size: 28px;
            font-weight: bold;
            color: #1a237e;
            margin-bottom: 10px;
        }}
        .title {{
            font-size: 42px;
            font-weight: bold;
            color: #1a237e;
            text-transform: uppercase;
            letter-spacing: 4px;
            margin: 20px 0;
        }}
        .subtitle {{
            font-size: 18px;
            color: #555;
            margin-bottom: 20px;
        }}
        .recipient-name {{
            font-size: 36px;
            font-weight: bold;
            color: #333;
            border-bottom: 2px solid #1a237e;
            display: inline-block;
            padding: 10px 40px;
            margin: 20px 0;
        }}
        .employee-code {{
            font-size: 14px;
            color: #666;
            margin-bottom: 20px;
        }}
        .course-info {{
            font-size: 18px;
            color: #444;
            margin: 20px 0;
            line-height: 1.8;
        }}
        .course-name {{
            font-size: 24px;
            font-weight: bold;
            color: #1a237e;
        }}
        .details-row {{
            display: flex;
            justify-content: center;
            gap: 40px;
            margin: 20px 0;
            flex-wrap: wrap;
        }}
        .detail-item {{
            text-align: center;
        }}
        .detail-label {{
            font-size: 12px;
            color: #888;
            text-transform: uppercase;
        }}
        .detail-value {{
            font-size: 16px;
            font-weight: bold;
            color: #333;
        }}
        .signatures {{
            display: flex;
            justify-content: space-around;
            margin-top: 40px;
            padding-top: 20px;
        }}
        .signature {{
            text-align: center;
            min-width: 200px;
        }}
        .signature-line {{
            border-top: 1px solid #333;
            margin-top: 60px;
            padding-top: 10px;
        }}
        .signature-name {{
            font-weight: bold;
            color: #333;
        }}
        .signature-title {{
            font-size: 12px;
            color: #666;
        }}
        .qr-section {{
            position: absolute;
            bottom: 60px;
            right: 60px;
        }}
        .qr-code {{
            width: 80px;
            height: 80px;
        }}
        .serial-number {{
            font-size: 10px;
            color: #888;
            margin-top: 5px;
        }}
        .print-btn {{
            position: fixed;
            top: 20px;
            right: 20px;
            background: #1a237e;
            color: white;
            border: none;
            padding: 15px 30px;
            font-size: 16px;
            cursor: pointer;
            border-radius: 5px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
        }}
        .print-btn:hover {{
            background: #303f9f;
        }}
        @media print {{
            .print-btn {{
                display: none;
            }}
            body {{
                background: white;
                padding: 0;
            }}
            .certificate {{
                box-shadow: none;
            }}
        }}
    </style>
</head>
<body>
    <button class=""print-btn"" onclick=""window.print()"">Download / Print Certificate</button>

    <div class=""certificate"">
        <div class=""border-outer"">
            <div class=""border-inner"">
                <div class=""header"">
                    <div class=""logo"">WorkForce360 Training Management</div>
                    <div class=""title"">Certificate of Completion</div>
                    <div class=""subtitle"">This is to certify that</div>
                </div>

                <div>
                    <div class=""recipient-name"">{staffName}</div>
                    <div class=""employee-code"">Employee Code: {staffCode}</div>
                </div>

                <div class=""course-info"">
                    <p>has successfully completed the training course</p>
                    <p class=""course-name"">{courseTitle}</p>
                    <p>Course Code: {courseCode}</p>
                </div>

                <div class=""details-row"">
                    <div class=""detail-item"">
                        <div class=""detail-label"">Course Period</div>
                        <div class=""detail-value"">{startDate:dd MMM yyyy} - {endDate:dd MMM yyyy}</div>
                    </div>
                    {(marks.HasValue ? $@"<div class=""detail-item"">
                        <div class=""detail-label"">Score</div>
                        <div class=""detail-value"">{marks:F1}%</div>
                    </div>" : "")}
                    <div class=""detail-item"">
                        <div class=""detail-label"">Attendance</div>
                        <div class=""detail-value"">{attendancePercentage:F1}%</div>
                    </div>
                </div>

                <div class=""signatures"">
                    <div class=""signature"">
                        <div class=""signature-line"">
                            <div class=""signature-name"">{trainerName}</div>
                            <div class=""signature-title"">Course Trainer</div>
                        </div>
                    </div>
                    <div class=""signature"">
                        <div class=""signature-line"">
                            <div class=""signature-name"">HR Manager</div>
                            <div class=""signature-title"">Human Resources</div>
                        </div>
                    </div>
                </div>

                <div class=""qr-section"">
                    <img src=""{qrCodePath}"" alt=""QR Code"" class=""qr-code"" />
                    <div class=""serial-number"">{serialNumber}</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    #region Date-wise Attendance CRUD Stored Procedure Methods

    public async Task<IEnumerable<AttendanceDateWiseDto>> GetAttendanceDateWiseSpAsync(int coursePlanId, int tenantId, DateTime? attendanceDate = null, int? staffId = null)
    {
        try
        {
            return await _unitOfWork.CourseAttendanceDateWise.GetAttendanceDateWiseSpAsync(coursePlanId, tenantId, attendanceDate, staffId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting date-wise attendance from stored procedure for CoursePlanId: {CoursePlanId}, TenantId: {TenantId}",
                coursePlanId, tenantId);
            throw;
        }
    }

    public async Task<AttendanceOperationResponse> CreateAttendanceDateWiseSpAsync(CreateAttendanceDateWiseRequest request, int tenantId, int? createdBy = null)
    {
        try
        {
            return await _unitOfWork.CourseAttendanceDateWise.CreateAttendanceDateWiseSpAsync(request, tenantId, createdBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating date-wise attendance via stored procedure for CoursePlanId: {CoursePlanId}, StaffId: {StaffId}",
                request.CoursePlanId, request.StaffId);
            return new AttendanceOperationResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<AttendanceOperationResponse> UpdateAttendanceDateWiseSpAsync(UpdateAttendanceDateWiseByIdRequest request, int tenantId, int? updatedBy = null)
    {
        try
        {
            return await _unitOfWork.CourseAttendanceDateWise.UpdateAttendanceDateWiseSpAsync(request, tenantId, updatedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating date-wise attendance via stored procedure for AttendanceId: {AttendanceId}",
                request.AttendanceId);
            return new AttendanceOperationResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<AttendanceOperationResponse> DeleteAttendanceDateWiseSpAsync(int attendanceId, int tenantId, int? updatedBy = null)
    {
        try
        {
            return await _unitOfWork.CourseAttendanceDateWise.DeleteAttendanceDateWiseSpAsync(attendanceId, tenantId, updatedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting date-wise attendance via stored procedure for AttendanceId: {AttendanceId}",
                attendanceId);
            return new AttendanceOperationResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<AttendanceOperationResponse> BulkMarkAttendanceDateWiseSpAsync(BulkAttendanceDateWiseRequest request, int tenantId, int? createdBy = null)
    {
        try
        {
            return await _unitOfWork.CourseAttendanceDateWise.BulkMarkAttendanceDateWiseSpAsync(request, tenantId, createdBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk marking date-wise attendance via stored procedure for CoursePlanId: {CoursePlanId}, Date: {AttendanceDate}",
                request.CoursePlanId, request.AttendanceDate);
            return new AttendanceOperationResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<(IEnumerable<AttendanceSummaryByStaffDto> StaffSummary, IEnumerable<DailyAttendanceSummaryDto> DailySummary)> GetAttendanceSummaryByCoursePlanSpAsync(int coursePlanId, int tenantId)
    {
        try
        {
            return await _unitOfWork.CourseAttendanceDateWise.GetAttendanceSummaryByCoursePlanSpAsync(coursePlanId, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting attendance summary from stored procedure for CoursePlanId: {CoursePlanId}, TenantId: {TenantId}",
                coursePlanId, tenantId);
            throw;
        }
    }

    #endregion

    #region My Courses

    public async Task<IEnumerable<MyCourseSummaryDto>> GetMyCoursesAsync(int staffId, int tenantId)
    {
        try
        {
            var participants = await _unitOfWork.CourseParticipant.FindAsync(
                cp => cp.StaffId == staffId && cp.TenantId == tenantId && cp.IsActive);

            var result = new List<MyCourseSummaryDto>();

            foreach (var participant in participants)
            {
                var coursePlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(
                    participant.CoursePlanId, tenantId);
                if (coursePlan == null) continue;

                var courseResult = await _unitOfWork.CourseResult.GetByCoursePlanStaffAsync(
                    participant.CoursePlanId, staffId, tenantId);

                result.Add(new MyCourseSummaryDto
                {
                    CoursePlanId = coursePlan.Id,
                    CourseId = coursePlan.CourseId,
                    CourseTitle = coursePlan.Course?.Title ?? string.Empty,
                    CourseCode = coursePlan.Course?.Code,
                    CourseType = coursePlan.Course?.CourseType?.Name,
                    CourseCategory = coursePlan.Course?.CourseCategory?.Name,
                    CourseDuration = coursePlan.Course?.Duration ?? 0,
                    StartDate = coursePlan.StartDate,
                    EndDate = coursePlan.EndDate,
                    StartTime = coursePlan.StartTime,
                    EndTime = coursePlan.EndTime,
                    Venue = coursePlan.Venue,
                    TrainerName = coursePlan.Trainer?.Name ?? string.Empty,
                    IsCompleted = coursePlan.IsCompleted,
                    IsActive = coursePlan.IsActive,
                    ResultStatus = courseResult?.ResultStatus,
                    AttendancePercentage = courseResult?.AttendancePercentage ?? 0,
                    Marks = courseResult?.Marks,
                    CertificatePath = courseResult?.CertificatePath,
                    CertificateSerialNumber = courseResult?.CertificateSerialNumber
                });
            }

            return result.OrderByDescending(r => r.StartDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my courses for StaffId: {StaffId}", staffId);
            return Enumerable.Empty<MyCourseSummaryDto>();
        }
    }

    #endregion

    #region Staff Certificate Methods

    public async Task<IEnumerable<StaffCertificateDto>> GetCertificatesByStaffIdAsync(int staffId, int tenantId)
    {
        try
        {
            return await _unitOfWork.CourseResult.GetCertificatesByStaffIdAsync(staffId, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certificates for StaffId: {StaffId}, TenantId: {TenantId}", staffId, tenantId);
            return Enumerable.Empty<StaffCertificateDto>();
        }
    }

    #endregion
}
