using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class LearningPathRepository : Repository<LearningPath>, ILearningPathRepository
{
    public LearningPathRepository(HRMSDbContext context) : base(context) { }

    public async Task<IEnumerable<LearningPath>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        var query = _dbSet
            .Include(lp => lp.JobTitle)
            .Include(lp => lp.Courses.OrderBy(c => c.SortOrder))
                .ThenInclude(lpc => lpc.Course).ThenInclude(c => c.CourseType)
            .Where(lp => lp.TenantId == tenantId);

        if (!includeInactive) query = query.Where(lp => lp.IsActive);
        return await query.OrderBy(lp => lp.Title).ToListAsync();
    }

    public async Task<LearningPath?> GetByIdAsync(int pathId, int tenantId)
    {
        return await _dbSet
            .Include(lp => lp.JobTitle)
            .Include(lp => lp.Courses.OrderBy(c => c.SortOrder))
                .ThenInclude(lpc => lpc.Course).ThenInclude(c => c.CourseType)
            .FirstOrDefaultAsync(lp => lp.LearningPathId == pathId && lp.TenantId == tenantId);
    }
}
