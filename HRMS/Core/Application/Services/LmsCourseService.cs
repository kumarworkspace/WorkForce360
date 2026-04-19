using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class LmsCourseService : ILmsCourseService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LmsCourseService> _logger;

    public LmsCourseService(IUnitOfWork uow, ILogger<LmsCourseService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IEnumerable<LmsCourseDto>> GetAllAsync(int tenantId, bool includeInactive = false)
    {
        var courses = await _uow.LmsCourse.GetAllActiveAsync(tenantId);
        return courses.Select(MapToDto);
    }

    public async Task<LmsCourseDto?> GetByIdAsync(int courseId, int tenantId)
    {
        var course = await _uow.LmsCourse.GetByIdAsync(courseId, tenantId);
        return course == null ? null : MapToDto(course);
    }

    public async Task<(bool Success, string Message, int CourseId)> CreateAsync(CreateLmsCourseRequest req)
    {
        try
        {
            var course = new LmsCourse
            {
                TenantId        = req.TenantId,
                Title           = req.Title,
                Description     = req.Description,
                CourseTypeId    = req.CourseTypeId,
                DifficultyId    = req.DifficultyId,
                ContentUrl      = req.ContentUrl,
                DurationMinutes = req.DurationMinutes,
                Tags            = req.Tags,
                ThumbnailPath   = req.ThumbnailPath,
                Objectives      = req.Objectives,
                IsActive        = true,
                CreatedBy       = req.CreatedBy,
                CreatedDate     = DateTime.UtcNow
            };
            await _uow.LmsCourse.AddAsync(course);
            await _uow.SaveChangesAsync();
            return (true, "Course created successfully.", course.LmsCourseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating LMS course for tenant {TenantId}", req.TenantId);
            return (false, "Failed to create course.", 0);
        }
    }

    public async Task<(bool Success, string Message)> UpdateAsync(UpdateLmsCourseRequest req)
    {
        try
        {
            var course = await _uow.LmsCourse.GetByIdAsync(req.LmsCourseId, req.TenantId);
            if (course == null) return (false, "Course not found.");

            course.Title           = req.Title;
            course.Description     = req.Description;
            course.CourseTypeId    = req.CourseTypeId;
            course.DifficultyId    = req.DifficultyId;
            course.ContentUrl      = req.ContentUrl;
            course.DurationMinutes = req.DurationMinutes;
            course.Tags            = req.Tags;
            course.ThumbnailPath   = req.ThumbnailPath;
            course.Objectives      = req.Objectives;
            course.UpdatedBy       = req.UpdatedBy;
            course.UpdatedDate     = DateTime.UtcNow;

            await _uow.LmsCourse.UpdateAsync(course);
            await _uow.SaveChangesAsync();
            return (true, "Course updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating LMS course {CourseId}", req.LmsCourseId);
            return (false, "Failed to update course.");
        }
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int courseId, int tenantId, string deletedBy)
    {
        try
        {
            var course = await _uow.LmsCourse.GetByIdAsync(courseId, tenantId);
            if (course == null) return (false, "Course not found.");

            course.IsActive    = false;
            course.UpdatedBy   = deletedBy;
            course.UpdatedDate = DateTime.UtcNow;
            await _uow.LmsCourse.UpdateAsync(course);
            await _uow.SaveChangesAsync();
            return (true, "Course deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting LMS course {CourseId}", courseId);
            return (false, "Failed to delete course.");
        }
    }

    public async Task<IEnumerable<LmsModuleDto>> GetModulesAsync(int courseId, int tenantId)
    {
        var modules = await _uow.LmsModule.GetByCourseIdAsync(courseId);
        return modules.Select(m => new LmsModuleDto
        {
            LmsModuleId     = m.LmsModuleId,
            LmsCourseId     = m.LmsCourseId,
            Title           = m.Title,
            Description     = m.Description,
            ContentUrl      = m.ContentUrl,
            ContentType     = m.ContentType,
            DurationMinutes = m.DurationMinutes,
            SortOrder       = m.SortOrder,
            IsActive        = m.IsActive
        });
    }

    public async Task<(bool Success, string Message, int ModuleId)> CreateModuleAsync(CreateLmsModuleRequest req)
    {
        try
        {
            var module = new LmsModule
            {
                LmsCourseId     = req.LmsCourseId,
                TenantId        = req.TenantId,
                Title           = req.Title,
                Description     = req.Description,
                ContentUrl      = req.ContentUrl,
                ContentType     = req.ContentType,
                DurationMinutes = req.DurationMinutes,
                SortOrder       = req.SortOrder,
                IsActive        = true,
                CreatedBy       = req.CreatedBy,
                CreatedDate     = DateTime.UtcNow
            };
            await _uow.LmsModule.AddAsync(module);
            await _uow.SaveChangesAsync();
            return (true, "Module created successfully.", module.LmsModuleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating LMS module for course {CourseId}", req.LmsCourseId);
            return (false, "Failed to create module.", 0);
        }
    }

    public async Task<(bool Success, string Message)> DeleteModuleAsync(int moduleId, int tenantId, string deletedBy)
    {
        try
        {
            var module = await _uow.LmsModule.GetByIdAsync(moduleId);
            if (module == null || module.TenantId != tenantId) return (false, "Module not found.");

            module.IsActive    = false;
            module.UpdatedBy   = deletedBy;
            module.UpdatedDate = DateTime.UtcNow;
            await _uow.LmsModule.UpdateAsync(module);
            await _uow.SaveChangesAsync();
            return (true, "Module deleted.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting LMS module {ModuleId}", moduleId);
            return (false, "Failed to delete module.");
        }
    }

    public async Task<(bool Success, string Message)> ReorderModulesAsync(int courseId, int tenantId, List<int> orderedModuleIds)
    {
        try
        {
            var modules = (await _uow.LmsModule.GetByCourseIdAsync(courseId)).ToList();
            for (int i = 0; i < orderedModuleIds.Count; i++)
            {
                var module = modules.FirstOrDefault(m => m.LmsModuleId == orderedModuleIds[i]);
                if (module != null)
                {
                    module.SortOrder = i + 1;
                    await _uow.LmsModule.UpdateAsync(module);
                }
            }
            await _uow.SaveChangesAsync();
            return (true, "Modules reordered.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering modules for course {CourseId}", courseId);
            return (false, "Failed to reorder modules.");
        }
    }

    private static LmsCourseDto MapToDto(LmsCourse c) => new()
    {
        LmsCourseId     = c.LmsCourseId,
        TenantId        = c.TenantId,
        Title           = c.Title,
        Description     = c.Description,
        CourseTypeId    = c.CourseTypeId,
        CourseTypeName  = c.CourseType?.ValueName,
        DifficultyId    = c.DifficultyId,
        DifficultyName  = c.Difficulty?.ValueName,
        ContentUrl      = c.ContentUrl,
        DurationMinutes = c.DurationMinutes,
        Tags            = c.Tags,
        ThumbnailPath   = c.ThumbnailPath,
        Objectives      = c.Objectives,
        IsActive        = c.IsActive,
        ModuleCount     = c.Modules?.Count(m => m.IsActive) ?? 0,
        EnrollmentCount = c.Enrollments?.Count(e => e.IsActive) ?? 0
    };
}
