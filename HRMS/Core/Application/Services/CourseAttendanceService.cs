using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class CourseAttendanceService : ICourseAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CourseAttendanceService> _logger;

    public CourseAttendanceService(
        IUnitOfWork unitOfWork,
        ILogger<CourseAttendanceService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<MarkAttendanceResponse> MarkAttendanceAsync(
        MarkAttendanceRequest request,
        int tenantId,
        int userId)
    {
        try
        {
            _logger.LogInformation(
                "Marking attendance for User: {UserId}, CoursePlan: {CoursePlanId}, Staff: {StaffId}, Tenant: {TenantId}",
                request.UserId, request.CoursePlanId, request.StaffId, tenantId);

            // Validate attendance time window before recording
            var coursePlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(request.CoursePlanId, tenantId);
            if (coursePlan == null)
            {
                return new MarkAttendanceResponse
                {
                    AttendanceId = 0,
                    Message = "Course plan not found.",
                    Success = false
                };
            }

            var now = DateTime.Now;
            var today = now.Date;

            // Course must be within its date range
            if (today < coursePlan.StartDate.Date || today > coursePlan.EndDate.Date)
            {
                return new MarkAttendanceResponse
                {
                    AttendanceId = 0,
                    Message = $"Invalid time for attendance. This course runs {coursePlan.StartDate:dd MMM yyyy} – {coursePlan.EndDate:dd MMM yyyy}.",
                    Success = false
                };
            }

            // Valid windows: [StartTime - 1h, StartTime + 1h] OR [EndTime - 1h, EndTime + 1h]
            var currentTime = now.TimeOfDay;
            var morningOpen  = coursePlan.StartTime - TimeSpan.FromHours(1);
            var morningClose = coursePlan.StartTime + TimeSpan.FromHours(1);
            var eveningOpen  = coursePlan.EndTime   - TimeSpan.FromHours(1);
            var eveningClose = coursePlan.EndTime   + TimeSpan.FromHours(1);

            var inMorningWindow = currentTime >= morningOpen && currentTime <= morningClose;
            var inEveningWindow = currentTime >= eveningOpen && currentTime <= eveningClose;

            if (!inMorningWindow && !inEveningWindow)
            {
                _logger.LogWarning(
                    "Attendance scan outside valid window. CoursePlanId={CoursePlanId}, StaffId={StaffId}, Time={Time}, MorningWindow={MOpen}-{MClose}, EveningWindow={EOpen}-{EClose}",
                    request.CoursePlanId, request.StaffId, currentTime, morningOpen, morningClose, eveningOpen, eveningClose);

                return new MarkAttendanceResponse
                {
                    AttendanceId = 0,
                    Message = $"Invalid time for attendance. Valid windows: {FormatTime(morningOpen)}–{FormatTime(morningClose)} or {FormatTime(eveningOpen)}–{FormatTime(eveningClose)}.",
                    Success = false
                };
            }

            var result = await _unitOfWork.CourseAttendance.MarkAttendanceAsync(
                request.UserId,
                request.CoursePlanId,
                request.StaffId,
                tenantId,
                request.CheckInTime,
                userId);

            if (result.AttendanceId > 0)
            {
                result.Success = true;
                _logger.LogInformation(
                    "Attendance marked successfully. AttendanceId: {AttendanceId}",
                    result.AttendanceId);

                // Also upsert into CourseAttendance_DateWise so the attendance grid reflects QR scan
                await UpsertDateWiseAttendanceAsync(request, tenantId, today, userId);
            }
            else
            {
                result.Success = false;
                _logger.LogWarning(
                    "Failed to mark attendance: {Message}",
                    result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error marking attendance for User: {UserId}, CoursePlan: {CoursePlanId}",
                request.UserId, request.CoursePlanId);

            return new MarkAttendanceResponse
            {
                AttendanceId = -1,
                Message = "An error occurred while marking attendance",
                Success = false
            };
        }
    }

    private async Task UpsertDateWiseAttendanceAsync(
        MarkAttendanceRequest request, int tenantId, DateTime today, int userId)
    {
        try
        {
            // Check if a date-wise record already exists for this participant + date
            var existing = await _unitOfWork.CourseAttendanceDateWise
                .GetByCoursePlanStaffDateAsync(request.CoursePlanId, request.StaffId, today, tenantId);

            if (existing != null)
            {
                // Already marked — update to Present
                existing.IsPresent    = true;
                existing.UpdatedDate  = DateTime.Now;
                existing.UpdatedBy    = userId;
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                // Insert new Present record for today
                var record = new CourseAttendanceDateWise
                {
                    CoursePlanId   = request.CoursePlanId,
                    StaffId        = request.StaffId,
                    AttendanceDate = today,
                    IsPresent      = true,
                    TenantId       = tenantId,
                    IsActive       = true,
                    CreatedDate    = DateTime.Now,
                    CreatedBy      = userId
                };
                await _unitOfWork.CourseAttendanceDateWise.AddAsync(record);
                await _unitOfWork.SaveChangesAsync();
            }

            _logger.LogInformation(
                "DateWise attendance upserted for CoursePlanId={CoursePlanId}, StaffId={StaffId}, Date={Date}",
                request.CoursePlanId, request.StaffId, today);
        }
        catch (Exception ex)
        {
            // Log but don't fail the main QR response — the Course_Attendance record was already saved
            _logger.LogError(ex,
                "Error upserting date-wise attendance for CoursePlanId={CoursePlanId}, StaffId={StaffId}",
                request.CoursePlanId, request.StaffId);
        }
    }

    public async Task<IEnumerable<CourseAttendanceDto>> GetAttendanceListAsync(GetAttendanceRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Fetching attendance list for Tenant: {TenantId}, CoursePlan: {CoursePlanId}",
                request.TenantId, request.CoursePlanId);

            var attendanceList = await _unitOfWork.CourseAttendance.GetCourseAttendanceAsync(
                request.TenantId,
                request.CoursePlanId,
                request.UserId,
                request.FromDate,
                request.ToDate);

            return attendanceList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching attendance list for Tenant: {TenantId}",
                request.TenantId);
            return Enumerable.Empty<CourseAttendanceDto>();
        }
    }

    private static string FormatTime(TimeSpan t)
    {
        var dt = DateTime.Today.Add(t < TimeSpan.Zero ? TimeSpan.Zero : t);
        return dt.ToString("hh:mm tt");
    }

    public async Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceByCoursePlanAsync(
        int coursePlanId,
        int tenantId)
    {
        try
        {
            _logger.LogInformation(
                "Fetching attendance summary for CoursePlan: {CoursePlanId}, Tenant: {TenantId}",
                coursePlanId, tenantId);

            var attendanceSummary = await _unitOfWork.CourseAttendance.GetAttendanceByCoursePlanAsync(
                coursePlanId,
                tenantId);

            return attendanceSummary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error fetching attendance summary for CoursePlan: {CoursePlanId}",
                coursePlanId);
            return Enumerable.Empty<AttendanceSummaryDto>();
        }
    }
}
