using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class CourseRegistrationService : ICourseRegistrationService
{
    private readonly ILogger<CourseRegistrationService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;
    private const string UploadsFolder = "uploads";
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB for course files
    private static readonly string[] AllowedCourseFileExtensions = { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx" };

    public CourseRegistrationService(
        ILogger<CourseRegistrationService> logger,
        IUnitOfWork unitOfWork,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<CourseDto?> GetByIdAsync(int courseId, int tenantId)
    {
        try
        {
            var course = await _unitOfWork.CourseRegistration.GetByIdWithDetailsAsync(courseId, tenantId);
            if (course == null) return null;

            if (course.TenantId != tenantId)
            {
                _logger.LogWarning("Tenant mismatch: Course {CourseId} belongs to tenant {CourseTenantId}, requested tenant {RequestedTenantId}",
                    courseId, course.TenantId, tenantId);
                throw new UnauthorizedAccessException("Access denied: Course does not belong to your tenant.");
            }

            return MapToDto(course);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course by ID: {CourseId} for tenant: {TenantId}", courseId, tenantId);
            throw;
        }
    }

    public async Task<PagedResult<CourseListDto>> GetPagedAsync(
        int tenantId,
        int pageNumber,
        int pageSize,
        int? courseTypeId = null,
        int? courseCategoryId = null,
        int? trainerId = null,
        bool? isActive = null,
        string? searchText = null,
        int? createdBy = null,
        int? staffId = null)
    {
        try
        {
            var allCourses = await _unitOfWork.CourseRegistration.GetFilteredCoursesAsync(
                tenantId, courseTypeId, courseCategoryId, trainerId, isActive, searchText);

            // Apply role-based filtering
            // If createdBy is provided: show courses created by this user
            // If staffId is provided: show courses assigned to this trainer (TrainerId = staffId)
            // Super Admin: both createdBy and staffId are null, so no filtering applied
            if (createdBy.HasValue || staffId.HasValue)
            {
                allCourses = allCourses.Where(c =>
                    (createdBy.HasValue && c.CreatedBy == createdBy.Value) ||
                    (staffId.HasValue && c.TrainerId == staffId.Value)
                ).ToList();
            }

            var totalCount = allCourses.Count();

            var pagedCourses = allCourses
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = pagedCourses.Select(c => new CourseListDto
            {
                CourseId = c.CourseId,
                Code = c.Code,
                CourseCode = c.CourseCode,
                Title = c.Title,
                TrainingModule = c.TrainingModule,
                CourseType = c.CourseType?.Name ?? string.Empty,
                CourseCategory = c.CourseCategory?.Name ?? string.Empty,
                TrainerName = c.Trainer?.Name ?? string.Empty,
                Duration = c.Duration,
                ValidityPeriod = c.ValidityPeriodType?.Name ?? string.Empty,
                IsActive = c.IsActive,
                CreatedDate = c.CreatedDate
            }).ToList();

            return new PagedResult<CourseListDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged courses for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<CourseDto> CreateAsync(CreateCourseRequest request, int tenantId, int userId, string? ipAddress)
    {
        ValidateRequest(request);

        int createdCourseId = 0;

        try
        {
            // Check if code already exists for this tenant
            var codeExists = await _unitOfWork.CourseRegistration.CodeExistsAsync(request.Code, tenantId);
            if (codeExists)
            {
                throw new InvalidOperationException($"Course code '{request.Code}' already exists for your organization.");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var course = new CourseRegistration
                {
                    Code = request.Code.Trim(),
                    CourseCode = request.CourseCode.Trim(),
                    Title = request.Title.Trim(),
                    TrainingModule = request.TrainingModule.Trim(),
                    CourseTypeId = request.CourseTypeId,
                    CourseCategoryId = request.CourseCategoryId,
                    TrainerId = request.TrainerId,
                    Duration = request.Duration,
                    ValidityPeriod = request.ValidityPeriod,
                    UploadFilePath = request.UploadFilePath,
                    TenantId = tenantId,
                    IsActive = true,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                var createdCourse = await _unitOfWork.CourseRegistration.AddAsync(course);
                await _unitOfWork.SaveChangesAsync();
                createdCourseId = createdCourse.CourseId;

                // Log audit
                await LogAuditAsync(ActionType.Create, userId, tenantId, ipAddress,
                    $"Created course: {course.Title} (Code: {course.Code})");
            });

            var result = await GetByIdAsync(createdCourseId, tenantId);
            return result ?? throw new InvalidOperationException("Failed to retrieve created course.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<CourseDto> UpdateAsync(UpdateCourseRequest request, int tenantId, int userId, string? ipAddress)
    {
        ValidateRequest(request);

        try
        {
            var existingCourse = await _unitOfWork.CourseRegistration.GetByIdWithDetailsAsync(request.CourseId, tenantId);
            if (existingCourse == null)
            {
                throw new InvalidOperationException("Course not found.");
            }

            if (existingCourse.TenantId != tenantId)
            {
                throw new UnauthorizedAccessException("Access denied: Course does not belong to your tenant.");
            }

            // Check if code already exists for another course
            var codeExists = await _unitOfWork.CourseRegistration.CodeExistsAsync(request.Code, tenantId, request.CourseId);
            if (codeExists)
            {
                throw new InvalidOperationException($"Course code '{request.Code}' already exists for your organization.");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var oldValues = $"Code: {existingCourse.Code}, Title: {existingCourse.Title}";

                existingCourse.Code = request.Code.Trim();
                existingCourse.CourseCode = request.CourseCode.Trim();
                existingCourse.Title = request.Title.Trim();
                existingCourse.TrainingModule = request.TrainingModule.Trim();
                existingCourse.CourseTypeId = request.CourseTypeId;
                existingCourse.CourseCategoryId = request.CourseCategoryId;
                existingCourse.TrainerId = request.TrainerId;
                existingCourse.Duration = request.Duration;
                existingCourse.ValidityPeriod = request.ValidityPeriod;
                existingCourse.UploadFilePath = request.UploadFilePath;
                existingCourse.UpdatedBy = userId;
                existingCourse.UpdatedDate = DateTime.Now;

                await _unitOfWork.CourseRegistration.UpdateAsync(existingCourse);

                var newValues = $"Code: {existingCourse.Code}, Title: {existingCourse.Title}";

                // Log audit
                await LogAuditAsync(ActionType.Update, userId, tenantId, ipAddress,
                    $"Updated course {existingCourse.CourseId}. Old: [{oldValues}] New: [{newValues}]");
            });

            var result = await GetByIdAsync(request.CourseId, tenantId);
            return result ?? throw new InvalidOperationException("Failed to retrieve updated course.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseId} for tenant: {TenantId}", request.CourseId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int courseId, int tenantId, int userId, string? ipAddress)
    {
        try
        {
            var course = await _unitOfWork.CourseRegistration.GetByIdWithDetailsAsync(courseId, tenantId);
            if (course == null)
            {
                throw new InvalidOperationException("Course not found.");
            }

            if (course.TenantId != tenantId)
            {
                throw new UnauthorizedAccessException("Access denied: Course does not belong to your tenant.");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Soft delete
                course.IsActive = false;
                course.UpdatedBy = userId;
                course.UpdatedDate = DateTime.Now;

                await _unitOfWork.CourseRegistration.UpdateAsync(course);

                // Log audit
                await LogAuditAsync(ActionType.Delete, userId, tenantId, ipAddress,
                    $"Deleted course: {course.Title} (Code: {course.Code})");
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting course {CourseId} for tenant: {TenantId}", courseId, tenantId);
            throw;
        }
    }

    public async Task<string> UploadCourseFileAsync(int courseId, int tenantId, Stream fileStream, string fileName, int userId)
    {
        try
        {
            var course = await _unitOfWork.CourseRegistration.GetByIdWithDetailsAsync(courseId, tenantId);
            if (course == null)
            {
                throw new InvalidOperationException("Course not found.");
            }

            if (course.TenantId != tenantId)
            {
                throw new UnauthorizedAccessException("Access denied: Course does not belong to your tenant.");
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!AllowedCourseFileExtensions.Contains(extension))
            {
                throw new ArgumentException($"File type {extension} is not allowed for course files. Allowed types: {string.Join(", ", AllowedCourseFileExtensions)}");
            }

            var folderPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath,
                UploadsFolder, tenantId.ToString(), "courses", courseId.ToString());
            Directory.CreateDirectory(folderPath);

            var safeFileName = Path.GetFileNameWithoutExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}{extension}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamWriter);
            }

            var relativePath = Path.Combine(UploadsFolder, tenantId.ToString(), "courses",
                courseId.ToString(), uniqueFileName).Replace('\\', '/');

            // Delete old file if exists
            if (!string.IsNullOrEmpty(course.UploadFilePath))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath,
                    course.UploadFilePath);
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            // Update course with new file path
            course.UploadFilePath = relativePath;
            course.UpdatedBy = userId;
            course.UpdatedDate = DateTime.Now;
            await _unitOfWork.CourseRegistration.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<bool> DeleteCourseFileAsync(int courseId, int tenantId, int userId)
    {
        try
        {
            var course = await _unitOfWork.CourseRegistration.GetByIdWithDetailsAsync(courseId, tenantId);
            if (course == null)
            {
                throw new InvalidOperationException("Course not found.");
            }

            if (course.TenantId != tenantId)
            {
                throw new UnauthorizedAccessException("Access denied: Course does not belong to your tenant.");
            }

            if (string.IsNullOrEmpty(course.UploadFilePath))
            {
                return false;
            }

            var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath,
                course.UploadFilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            course.UploadFilePath = null;
            course.UpdatedBy = userId;
            course.UpdatedDate = DateTime.Now;
            await _unitOfWork.CourseRegistration.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file for course {CourseId}", courseId);
            throw;
        }
    }

    public async Task<CourseStatisticsDto> GetCourseStatisticsAsync(int tenantId, int? createdBy = null, int? staffId = null)
    {
        try
        {
            var allCourses = await _unitOfWork.CourseRegistration.GetFilteredCoursesAsync(tenantId);

            // Apply role-based filtering
            IEnumerable<CourseRegistration> filteredCourses = allCourses;
            if (createdBy.HasValue || staffId.HasValue)
            {
                filteredCourses = allCourses.Where(c =>
                    (createdBy.HasValue && c.CreatedBy == createdBy.Value) ||
                    (staffId.HasValue && c.TrainerId == staffId.Value)
                ).ToList();
            }

            // Created Courses Count: courses created by the user (for Trainer role)
            var createdCoursesCount = createdBy.HasValue
                ? allCourses.Count(c => c.CreatedBy == createdBy.Value)
                : allCourses.Count();

            // Assigned Courses Count: courses assigned to the user as trainer
            var assignedCoursesCount = staffId.HasValue
                ? allCourses.Count(c => c.TrainerId == staffId.Value)
                : 0;

            // Active Courses Count: active courses visible to the user
            var activeCoursesCount = filteredCourses.Count(c => c.IsActive);

            // Total Training Hours: sum of duration for visible courses
            var totalTrainingHours = filteredCourses.Sum(c => c.Duration);

            return new CourseStatisticsDto
            {
                CreatedCoursesCount = createdCoursesCount,
                AssignedCoursesCount = assignedCoursesCount,
                ActiveCoursesCount = activeCoursesCount,
                TotalTrainingHours = totalTrainingHours
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course statistics for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<List<MonthlyStatisticsDto>> GetMonthlyStatisticsAsync(int tenantId, int year, int? createdBy = null, int? staffId = null)
    {
        try
        {
            var allCourses = await _unitOfWork.CourseRegistration.GetFilteredCoursesAsync(tenantId);

            // Apply role-based filtering
            IEnumerable<CourseRegistration> filteredCourses = allCourses;
            if (createdBy.HasValue || staffId.HasValue)
            {
                filteredCourses = allCourses.Where(c =>
                    (createdBy.HasValue && c.CreatedBy == createdBy.Value) ||
                    (staffId.HasValue && c.TrainerId == staffId.Value)
                ).ToList();
            }

            // Filter by year and group by month
            var monthlyData = filteredCourses
                .Where(c => c.CreatedDate.Year == year)
                .GroupBy(c => c.CreatedDate.Month)
                .Select(g => new MonthlyStatisticsDto
                {
                    Month = g.Key,
                    MonthName = new DateTime(year, g.Key, 1).ToString("MMM"),
                    Count = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            // Fill in missing months with zero counts
            var allMonths = Enumerable.Range(1, 12).Select(month => new MonthlyStatisticsDto
            {
                Month = month,
                MonthName = new DateTime(year, month, 1).ToString("MMM"),
                Count = monthlyData.FirstOrDefault(m => m.Month == month)?.Count ?? 0
            }).ToList();

            return allMonths;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly statistics for tenant: {TenantId}, year: {Year}", tenantId, year);
            throw;
        }
    }

    public async Task<List<YearlyStatisticsDto>> GetYearlyStatisticsAsync(int tenantId, int? createdBy = null, int? staffId = null)
    {
        try
        {
            var allCourses = await _unitOfWork.CourseRegistration.GetFilteredCoursesAsync(tenantId);

            // Apply role-based filtering
            IEnumerable<CourseRegistration> filteredCourses = allCourses;
            if (createdBy.HasValue || staffId.HasValue)
            {
                filteredCourses = allCourses.Where(c =>
                    (createdBy.HasValue && c.CreatedBy == createdBy.Value) ||
                    (staffId.HasValue && c.TrainerId == staffId.Value)
                ).ToList();
            }

            // Group by year
            var yearlyData = filteredCourses
                .GroupBy(c => c.CreatedDate.Year)
                .Select(g => new YearlyStatisticsDto
                {
                    Year = g.Key,
                    Count = g.Count()
                })
                .OrderBy(y => y.Year)
                .ToList();

            return yearlyData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting yearly statistics for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    private void ValidateRequest(CreateCourseRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Course Code is required.");

        if (string.IsNullOrWhiteSpace(request.CourseCode))
            throw new ArgumentException("Course Code is required.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.");

        if (string.IsNullOrWhiteSpace(request.TrainingModule))
            throw new ArgumentException("Training Module is required.");

        if (request.CourseTypeId <= 0)
            throw new ArgumentException("Course Type is required.");

        if (request.CourseCategoryId <= 0)
            throw new ArgumentException("Course Category is required.");

        if (request.TrainerId <= 0)
            throw new ArgumentException("Trainer is required.");

        if (request.Duration <= 0)
            throw new ArgumentException("Duration must be greater than 0.");

        if (request.ValidityPeriod <= 0)
            throw new ArgumentException("Validity Period is required.");
    }

    private CourseDto MapToDto(CourseRegistration course)
    {
        return new CourseDto
        {
            CourseId = course.CourseId,
            Code = course.Code,
            CourseCode = course.CourseCode,
            Title = course.Title,
            TrainingModule = course.TrainingModule,
            CourseTypeId = course.CourseTypeId,
            CourseTypeName = course.CourseType?.Name,
            CourseCategoryId = course.CourseCategoryId,
            CourseCategoryName = course.CourseCategory?.Name,
            TrainerId = course.TrainerId,
            TrainerName = course.Trainer?.Name,
            Duration = course.Duration,
            ValidityPeriod = course.ValidityPeriod,
            ValidityPeriodName = course.ValidityPeriodType?.Name,
            UploadFilePath = course.UploadFilePath,
            IsActive = course.IsActive,
            CreatedBy = course.CreatedBy,
            CreatedDate = course.CreatedDate,
            UpdatedBy = course.UpdatedBy,
            UpdatedDate = course.UpdatedDate
        };
    }

    private async Task LogAuditAsync(ActionType actionType, int userId, int tenantId, string? ipAddress, string description)
    {
        var auditLog = new AuditLog
        {
            TenantId = tenantId,
            UserId = userId,
            ActionType = actionType,
            Description = description,
            IPAddress = ipAddress,
            IsActive = true,
            CreatedBy = userId,
            CreatedDate = DateTime.Now
        };

        await _unitOfWork.AuditLogs.AddAsync(auditLog);
    }
}
