using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class LearningPathService : ILearningPathService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LearningPathService> _logger;

    public LearningPathService(IUnitOfWork uow, ILogger<LearningPathService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IEnumerable<LearningPathDto>> GetAllAsync(int tenantId, bool includeInactive = false)
    {
        var paths = await _uow.LearningPath.GetByTenantIdAsync(tenantId, includeInactive);
        return paths.Select(MapToDto);
    }

    public async Task<LearningPathDto?> GetByIdAsync(int pathId, int tenantId)
    {
        var path = await _uow.LearningPath.GetByIdAsync(pathId, tenantId);
        return path == null ? null : MapToDto(path);
    }

    public async Task<(bool Success, string Message, int PathId)> CreateAsync(CreateLearningPathRequest req)
    {
        try
        {
            var path = new LearningPath
            {
                TenantId    = req.TenantId,
                Title       = req.Title,
                Description = req.Description,
                JobTitleId  = req.JobTitleId,
                IsActive    = true,
                CreatedBy   = req.CreatedBy,
                CreatedDate = DateTime.UtcNow
            };

            for (int i = 0; i < req.CourseIds.Count; i++)
            {
                path.Courses.Add(new LearningPathCourse
                {
                    LmsCourseId = req.CourseIds[i],
                    TenantId    = req.TenantId,
                    SortOrder   = i + 1
                });
            }

            await _uow.LearningPath.AddAsync(path);
            await _uow.SaveChangesAsync();
            return (true, "Learning path created.", path.LearningPathId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating learning path for tenant {TenantId}", req.TenantId);
            return (false, "Failed to create learning path.", 0);
        }
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int pathId, CreateLearningPathRequest req)
    {
        try
        {
            var path = await _uow.LearningPath.GetByIdAsync(pathId, req.TenantId);
            if (path == null) return (false, "Learning path not found.");

            path.Title       = req.Title;
            path.Description = req.Description;
            path.JobTitleId  = req.JobTitleId;
            path.UpdatedBy   = req.CreatedBy;
            path.UpdatedDate = DateTime.UtcNow;

            path.Courses.Clear();
            for (int i = 0; i < req.CourseIds.Count; i++)
            {
                path.Courses.Add(new LearningPathCourse
                {
                    LearningPathId = pathId,
                    LmsCourseId    = req.CourseIds[i],
                    TenantId       = req.TenantId,
                    SortOrder      = i + 1
                });
            }

            await _uow.LearningPath.UpdateAsync(path);
            await _uow.SaveChangesAsync();
            return (true, "Learning path updated.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating learning path {PathId}", pathId);
            return (false, "Failed to update learning path.");
        }
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int pathId, int tenantId, string deletedBy)
    {
        try
        {
            var path = await _uow.LearningPath.GetByIdAsync(pathId, tenantId);
            if (path == null) return (false, "Learning path not found.");

            path.IsActive    = false;
            path.UpdatedBy   = deletedBy;
            path.UpdatedDate = DateTime.UtcNow;
            await _uow.LearningPath.UpdateAsync(path);
            await _uow.SaveChangesAsync();
            return (true, "Learning path deleted.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting learning path {PathId}", pathId);
            return (false, "Failed to delete learning path.");
        }
    }

    private static LearningPathDto MapToDto(LearningPath p) => new()
    {
        LearningPathId = p.LearningPathId,
        TenantId       = p.TenantId,
        Title          = p.Title,
        Description    = p.Description,
        JobTitleId     = p.JobTitleId,
        JobTitleName   = p.JobTitle?.ValueName,
        IsActive       = p.IsActive,
        Courses        = p.Courses.Select(lpc => new LearningPathCourseDto
        {
            LearningPathCourseId = lpc.LearningPathCourseId,
            LmsCourseId          = lpc.LmsCourseId,
            CourseTitle          = lpc.Course?.Title ?? string.Empty,
            SortOrder            = lpc.SortOrder,
            DurationMinutes      = lpc.Course?.DurationMinutes ?? 0
        }).ToList()
    };
}
