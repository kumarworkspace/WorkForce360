using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class LmsCourseRepository : Repository<LmsCourse>, ILmsCourseRepository
{
    public LmsCourseRepository(HRMSDbContext context) : base(context) { }

    public async Task<(IEnumerable<LmsCourse> Items, int TotalCount)> GetPagedAsync(
        int tenantId, string? search, int? courseTypeId, int? difficultyId, int pageNumber, int pageSize)
    {
        var query = _dbSet
            .Include(c => c.CourseType)
            .Include(c => c.Difficulty)
            .Where(c => c.TenantId == tenantId && c.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Title.Contains(search) || (c.Tags != null && c.Tags.Contains(search)));

        if (courseTypeId.HasValue)
            query = query.Where(c => c.CourseTypeId == courseTypeId);

        if (difficultyId.HasValue)
            query = query.Where(c => c.DifficultyId == difficultyId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<LmsCourse?> GetByIdAsync(int courseId, int tenantId)
    {
        return await _dbSet
            .Include(c => c.CourseType)
            .Include(c => c.Difficulty)
            .Include(c => c.Modules.Where(m => m.IsActive).OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(c => c.LmsCourseId == courseId && c.TenantId == tenantId);
    }

    public async Task<IEnumerable<LmsCourse>> GetAllActiveAsync(int tenantId)
    {
        return await _dbSet
            .Include(c => c.CourseType)
            .Where(c => c.TenantId == tenantId && c.IsActive)
            .OrderBy(c => c.Title)
            .ToListAsync();
    }
}
