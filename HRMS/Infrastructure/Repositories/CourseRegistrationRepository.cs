using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class CourseRegistrationRepository : Repository<CourseRegistration>, ICourseRegistrationRepository
{
    public CourseRegistrationRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<CourseRegistration?> GetByIdWithDetailsAsync(int courseId, int tenantId)
    {
        return await _dbSet
            .Where(c => c.CourseId == courseId && c.TenantId == tenantId)
            .Include(c => c.CourseType)
            .Include(c => c.CourseCategory)
            .Include(c => c.ValidityPeriodType)
            .Include(c => c.Trainer)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CourseRegistration>> GetByTenantIdAsync(int tenantId)
    {
        return await _dbSet
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .Include(c => c.CourseType)
            .Include(c => c.CourseCategory)
            .Include(c => c.ValidityPeriodType)
            .Include(c => c.Trainer)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> CodeExistsAsync(string code, int tenantId, int? excludeCourseId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var codeLower = code.ToLower().Trim();
        var query = _dbSet.Where(c =>
            c.Code != null &&
            c.Code.ToLower().Trim() == codeLower &&
            c.TenantId == tenantId);

        if (excludeCourseId.HasValue)
        {
            query = query.Where(c => c.CourseId != excludeCourseId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<CourseRegistration>> GetFilteredCoursesAsync(
        int tenantId,
        int? courseTypeId = null,
        int? courseCategoryId = null,
        int? trainerId = null,
        bool? isActive = null,
        string? searchText = null)
    {
        var query = _dbSet
            .Where(c => c.TenantId == tenantId)
            .Include(c => c.CourseType)
            .Include(c => c.CourseCategory)
            .Include(c => c.ValidityPeriodType)
            .Include(c => c.Trainer)
            .AsQueryable();

        if (courseTypeId.HasValue)
        {
            query = query.Where(c => c.CourseTypeId == courseTypeId.Value);
        }

        if (courseCategoryId.HasValue)
        {
            query = query.Where(c => c.CourseCategoryId == courseCategoryId.Value);
        }

        if (trainerId.HasValue)
        {
            query = query.Where(c => c.TrainerId == trainerId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var searchLower = searchText.ToLower();
            query = query.Where(c =>
                (c.Title != null && c.Title.ToLower().Contains(searchLower)) ||
                (c.Code != null && c.Code.ToLower().Contains(searchLower)));
        }

        return await query
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }
}
