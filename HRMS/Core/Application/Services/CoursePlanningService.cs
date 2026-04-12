using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace HRMS.Core.Application.Services;

public class CoursePlanningService : ICoursePlanningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CoursePlanningService> _logger;
    private readonly IHostEnvironment _environment;
    private const string UploadsFolder = "uploads";

    public CoursePlanningService(
        IUnitOfWork unitOfWork,
        ILogger<CoursePlanningService> logger,
        IHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _environment = environment;
    }

    public async Task<CoursePlanningDto?> GetByIdAsync(int id, int tenantId)
    {
        var coursePlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(id, tenantId);
        if (coursePlan == null)
            return null;

        return MapToDto(coursePlan);
    }

    public async Task<PagedResult<CoursePlanningListDto>> GetPagedAsync(
        int tenantId,
        int pageNumber,
        int pageSize,
        int? trainerId = null,
        int? courseId = null,
        bool? isActive = null,
        int? createdBy = null,
        int? staffId = null)
    {
        var allCoursePlans = await _unitOfWork.CoursePlanning.GetFilteredCoursePlansAsync(
            tenantId, trainerId, courseId, isActive);

        // Apply role-based filtering
        if (createdBy.HasValue || staffId.HasValue)
        {
            allCoursePlans = allCoursePlans.Where(cp =>
                (createdBy.HasValue && cp.CreatedBy == createdBy.Value) ||
                (staffId.HasValue && cp.TrainerId == staffId.Value)
            ).ToList();
        }

        var totalCount = allCoursePlans.Count();
        var coursePlans = allCoursePlans
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToListDto)
            .ToList();

        return new PagedResult<CoursePlanningListDto>
        {
            Items = coursePlans,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CoursePlanningDto> CreateAsync(CreateCoursePlanningRequest request, int tenantId, int userId, string? ipAddress)
    {
        // Validate date/time ranges
        ValidateDateTimeRange(request.StartDate, request.StartTime, request.EndDate, request.EndTime);

        // Check for conflicts
        var conflictRequest = new ConflictValidationRequest
        {
            TrainerId = request.TrainerId,
            StartDate = request.StartDate,
            StartTime = request.StartTime,
            EndDate = request.EndDate,
            EndTime = request.EndTime,
            TenantId = tenantId
        };
        var conflictResult = await ValidateConflictAsync(conflictRequest);
        if (conflictResult.HasConflict)
        {
            throw new InvalidOperationException(conflictResult.Message);
        }

        var coursePlan = new CoursePlanning
        {
            CourseId = request.CourseId,
            StartDate = request.StartDate,
            StartTime = request.StartTime,
            EndDate = request.EndDate,
            EndTime = request.EndTime,
            Venue = request.Venue,
            TrainerId = request.TrainerId,
            Remarks = request.Remarks,
            UploadFilePaths = request.UploadFilePaths != null && request.UploadFilePaths.Any()
                ? string.Join(";", request.UploadFilePaths)
                : null,
            TenantId = tenantId,
            IsActive = true,
            CreatedBy = userId,
            CreatedDate = DateTime.Now
        };

        await _unitOfWork.CoursePlanning.AddAsync(coursePlan);
        await _unitOfWork.SaveChangesAsync();

        // Auto-generate QR code for the new course plan
        try
        {
            await GenerateQRCodeAsync(coursePlan.Id, tenantId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to auto-generate QR code for course plan {Id}", coursePlan.Id);
        }

        var createdPlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(coursePlan.Id, tenantId);
        return MapToDto(createdPlan!);
    }

    public async Task<CoursePlanningDto> UpdateAsync(UpdateCoursePlanningRequest request, int tenantId, int userId, string? ipAddress)
    {
        var coursePlans = await _unitOfWork.CoursePlanning.FindAsync(cp => cp.Id == request.Id && cp.TenantId == tenantId);
        var coursePlan = coursePlans.FirstOrDefault();

        if (coursePlan == null)
            throw new InvalidOperationException("Course plan not found");

        // Validate date/time ranges
        ValidateDateTimeRange(request.StartDate, request.StartTime, request.EndDate, request.EndTime);

        // Check for conflicts (excluding current record)
        var conflictRequest = new ConflictValidationRequest
        {
            Id = request.Id,
            TrainerId = request.TrainerId,
            StartDate = request.StartDate,
            StartTime = request.StartTime,
            EndDate = request.EndDate,
            EndTime = request.EndTime,
            TenantId = tenantId
        };
        var conflictResult = await ValidateConflictAsync(conflictRequest);
        if (conflictResult.HasConflict)
        {
            throw new InvalidOperationException(conflictResult.Message);
        }

        // Update properties
        coursePlan.CourseId = request.CourseId;
        coursePlan.StartDate = request.StartDate;
        coursePlan.StartTime = request.StartTime;
        coursePlan.EndDate = request.EndDate;
        coursePlan.EndTime = request.EndTime;
        coursePlan.Venue = request.Venue;
        coursePlan.TrainerId = request.TrainerId;
        coursePlan.Remarks = request.Remarks;
        coursePlan.UploadFilePaths = request.UploadFilePaths != null && request.UploadFilePaths.Any()
            ? string.Join(";", request.UploadFilePaths)
            : null;
        coursePlan.UpdatedBy = userId;
        coursePlan.UpdatedDate = DateTime.Now;

        await _unitOfWork.CoursePlanning.UpdateAsync(coursePlan);
        await _unitOfWork.SaveChangesAsync();

        var updatedPlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(coursePlan.Id, tenantId);
        return MapToDto(updatedPlan!);
    }

    public async Task<bool> DeleteAsync(int id, int tenantId, int userId, string? ipAddress)
    {
        var coursePlans = await _unitOfWork.CoursePlanning.FindAsync(cp => cp.Id == id && cp.TenantId == tenantId);
        var coursePlan = coursePlans.FirstOrDefault();

        if (coursePlan == null)
            return false;

        // Soft delete
        coursePlan.IsActive = false;
        coursePlan.UpdatedBy = userId;
        coursePlan.UpdatedDate = DateTime.Now;

        await _unitOfWork.CoursePlanning.UpdateAsync(coursePlan);
        await _unitOfWork.SaveChangesAsync();

        // Delete associated files
        if (!string.IsNullOrEmpty(coursePlan.UploadFilePaths))
        {
            var filePaths = coursePlan.UploadFilePaths.Split(';');
            foreach (var filePath in filePaths)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
                }
            }
        }

        // Delete QR code if exists
        if (!string.IsNullOrEmpty(coursePlan.QRCodePath))
        {
            try
            {
                if (File.Exists(coursePlan.QRCodePath))
                {
                    File.Delete(coursePlan.QRCodePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete QR code: {QRCodePath}", coursePlan.QRCodePath);
            }
        }

        return true;
    }

    public async Task<ConflictValidationResult> ValidateConflictAsync(ConflictValidationRequest request)
    {
        var conflictCount = await _unitOfWork.CoursePlanning.ValidateConflictAsync(
            request.TenantId,
            request.TrainerId,
            request.StartDate,
            request.StartTime,
            request.EndDate,
            request.EndTime,
            request.Id);

        var result = new ConflictValidationResult
        {
            HasConflict = conflictCount > 0
        };

        if (result.HasConflict)
        {
            result.Message = $"The trainer has {conflictCount} conflicting schedule(s) during this time period.";

            // Load the conflicting schedules for display
            var allPlans = await _unitOfWork.CoursePlanning.GetFilteredCoursePlansAsync(
                request.TenantId, request.TrainerId, null, true);

            result.ConflictingSchedules = allPlans
                .Where(cp =>
                    cp.Id != request.Id &&
                    (
                        (request.StartDate >= cp.StartDate && request.StartDate <= cp.EndDate) ||
                        (request.EndDate >= cp.StartDate && request.EndDate <= cp.EndDate) ||
                        (request.StartDate <= cp.StartDate && request.EndDate >= cp.EndDate)
                    ))
                .Select(MapToListDto)
                .ToList();
        }

        return result;
    }

    public async Task<List<string>> UploadFilesAsync(int coursePlanningId, int tenantId, List<(Stream Stream, string FileName)> files, int userId)
    {
        var coursePlans = await _unitOfWork.CoursePlanning.FindAsync(cp => cp.Id == coursePlanningId && cp.TenantId == tenantId);
        var coursePlan = coursePlans.FirstOrDefault();

        if (coursePlan == null)
            throw new InvalidOperationException("Course plan not found");

        var uploadedPaths = new List<string>();
        var folderPath = Path.Combine(_environment.ContentRootPath ?? "",
            UploadsFolder, tenantId.ToString(), "courseplanning", coursePlanningId.ToString());

        Directory.CreateDirectory(folderPath);

        foreach (var (stream, fileName) in files)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            uploadedPaths.Add(filePath);
        }

        // Update the course plan with new file paths
        var existingPaths = string.IsNullOrEmpty(coursePlan.UploadFilePaths)
            ? new List<string>()
            : coursePlan.UploadFilePaths.Split(';').ToList();

        existingPaths.AddRange(uploadedPaths);
        coursePlan.UploadFilePaths = string.Join(";", existingPaths);
        coursePlan.UpdatedBy = userId;
        coursePlan.UpdatedDate = DateTime.Now;

        await _unitOfWork.CoursePlanning.UpdateAsync(coursePlan);
        await _unitOfWork.SaveChangesAsync();

        return uploadedPaths;
    }

    public async Task<bool> DeleteFileAsync(int coursePlanningId, int tenantId, string filePath, int userId)
    {
        var coursePlans = await _unitOfWork.CoursePlanning.FindAsync(cp => cp.Id == coursePlanningId && cp.TenantId == tenantId);
        var coursePlan = coursePlans.FirstOrDefault();

        if (coursePlan == null)
            return false;

        if (string.IsNullOrEmpty(coursePlan.UploadFilePaths))
            return false;

        var filePaths = coursePlan.UploadFilePaths.Split(';').ToList();
        if (!filePaths.Contains(filePath))
            return false;

        // Delete the physical file
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
        }

        // Remove from the list
        filePaths.Remove(filePath);
        coursePlan.UploadFilePaths = filePaths.Any() ? string.Join(";", filePaths) : null;
        coursePlan.UpdatedBy = userId;
        coursePlan.UpdatedDate = DateTime.Now;

        await _unitOfWork.CoursePlanning.UpdateAsync(coursePlan);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<string> GenerateQRCodeAsync(int coursePlanningId, int tenantId, int userId, string? baseUrl = null)
    {
        var coursePlan = await _unitOfWork.CoursePlanning.GetByIdWithDetailsAsync(coursePlanningId, tenantId);
        if (coursePlan == null)
            throw new InvalidOperationException("Course plan not found");

        // Delete existing QR code if it exists
        if (!string.IsNullOrEmpty(coursePlan.QRCodePath))
        {
            try
            {
                // Convert web path to file system path if needed
                var existingFilePath = coursePlan.QRCodePath;
                if (existingFilePath.StartsWith("/"))
                {
                    existingFilePath = Path.Combine(_environment.ContentRootPath ?? "", existingFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                }

                if (File.Exists(existingFilePath))
                {
                    File.Delete(existingFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete existing QR code: {QRCodePath}", coursePlan.QRCodePath);
            }
        }

        // Generate QR code content — encode the attendance mark URL so scanning opens the page directly
        var attendancePath = $"/tms/attendance/mark/{coursePlanningId}";
        var qrContent = !string.IsNullOrEmpty(baseUrl)
            ? $"{baseUrl.TrimEnd('/')}{attendancePath}"
            : attendancePath;

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrCodeData);
        using var qrCodeImage = qrCode.GetGraphic(20);

        // Save QR code image to file system
        var relativeFolder = Path.Combine(tenantId.ToString(), "courseplanning", coursePlanningId.ToString(), "qr");
        var folderPath = Path.Combine(_environment.ContentRootPath ?? "", UploadsFolder, relativeFolder);
        Directory.CreateDirectory(folderPath);

        var fileName = $"qrcode_{coursePlanningId}_{DateTime.Now:yyyyMMddHHmmss}.png";
        var fileSystemPath = Path.Combine(folderPath, fileName);

        qrCodeImage.Save(fileSystemPath, ImageFormat.Png);

        // Store web-accessible relative path (not file system path)
        var webPath = $"/uploads/{tenantId}/courseplanning/{coursePlanningId}/qr/{fileName}";

        // Update course plan with web-accessible QR code path
        coursePlan.QRCodePath = webPath;
        coursePlan.UpdatedBy = userId;
        coursePlan.UpdatedDate = DateTime.Now;

        await _unitOfWork.CoursePlanning.UpdateAsync(coursePlan);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("QR code generated for course plan {Id}: {WebPath}", coursePlanningId, webPath);

        return webPath;
    }

    #region Private Helper Methods

    private void ValidateDateTimeRange(DateTime startDate, TimeSpan startTime, DateTime endDate, TimeSpan endTime)
    {
        // Check if start date/time is in the past
        var startDateTime = startDate.Add(startTime);
        if (startDateTime < DateTime.Now)
        {
            throw new InvalidOperationException("Start date and time cannot be in the past.");
        }

        // Check if end date/time is after start date/time
        var endDateTime = endDate.Add(endTime);
        if (endDateTime <= startDateTime)
        {
            throw new InvalidOperationException("End date and time must be after start date and time.");
        }
    }

    private CoursePlanningDto MapToDto(CoursePlanning coursePlan)
    {
        return new CoursePlanningDto
        {
            Id = coursePlan.Id,
            CourseId = coursePlan.CourseId,
            CourseTitle = coursePlan.Course?.Title ?? string.Empty,
            CourseCode = coursePlan.Course?.Code ?? string.Empty,
            CourseNumber = coursePlan.Course?.CourseCode ?? string.Empty,
            TrainingModule = coursePlan.Course?.TrainingModule,
            StartDate = coursePlan.StartDate,
            StartTime = coursePlan.StartTime,
            EndDate = coursePlan.EndDate,
            EndTime = coursePlan.EndTime,
            Venue = coursePlan.Venue,
            TrainerId = coursePlan.TrainerId,
            TrainerName = coursePlan.Trainer?.Name ?? string.Empty,
            TrainerEmail = coursePlan.Trainer?.Email,
            TrainerPhone = coursePlan.Trainer?.PhoneNumber,
            Remarks = coursePlan.Remarks,
            UploadFilePaths = string.IsNullOrEmpty(coursePlan.UploadFilePaths)
                ? new List<string>()
                : coursePlan.UploadFilePaths.Split(';').ToList(),
            QRCodePath = coursePlan.QRCodePath,
            CourseType = coursePlan.Course?.CourseType?.Name,
            CourseCategory = coursePlan.Course?.CourseCategory?.Name,
            CourseDuration = coursePlan.Course?.Duration ?? 0,
            TenantId = coursePlan.TenantId,
            IsActive = coursePlan.IsActive,
            IsCompleted = coursePlan.IsCompleted,
            CreatedBy = coursePlan.CreatedBy,
            CreatedDate = coursePlan.CreatedDate,
            UpdatedBy = coursePlan.UpdatedBy,
            UpdatedDate = coursePlan.UpdatedDate
        };
    }

    private CoursePlanningListDto MapToListDto(CoursePlanning coursePlan)
    {
        return new CoursePlanningListDto
        {
            Id = coursePlan.Id,
            CourseId = coursePlan.CourseId,
            CourseTitle = coursePlan.Course?.Title ?? string.Empty,
            CourseCode = coursePlan.Course?.Code ?? string.Empty,
            CourseNumber = coursePlan.Course?.CourseCode ?? string.Empty,
            StartDate = coursePlan.StartDate,
            StartTime = coursePlan.StartTime,
            EndDate = coursePlan.EndDate,
            EndTime = coursePlan.EndTime,
            Venue = coursePlan.Venue,
            TrainerId = coursePlan.TrainerId,
            TrainerName = coursePlan.Trainer?.Name ?? string.Empty,
            TrainerEmail = coursePlan.Trainer?.Email,
            CourseType = coursePlan.Course?.CourseType?.Name,
            CourseCategory = coursePlan.Course?.CourseCategory?.Name,
            CourseDuration = coursePlan.Course?.Duration ?? 0,
            IsActive = coursePlan.IsActive,
            IsCompleted = coursePlan.IsCompleted,
            CreatedDate = coursePlan.CreatedDate
        };
    }

    #endregion

    public async Task<bool> UpdateCompletionStatusAsync(int id, int tenantId, bool isCompleted, int userId)
    {
        try
        {
            var coursePlans = await _unitOfWork.CoursePlanning.FindAsync(cp => cp.Id == id && cp.TenantId == tenantId);
            var coursePlan = coursePlans.FirstOrDefault();

            if (coursePlan == null)
                return false;

            coursePlan.IsCompleted = isCompleted;
            coursePlan.UpdatedBy = userId;
            coursePlan.UpdatedDate = DateTime.Now;

            await _unitOfWork.CoursePlanning.UpdateAsync(coursePlan);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating completion status for course plan {Id}", id);
            return false;
        }
    }
}
