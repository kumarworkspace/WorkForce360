using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class LmsModuleRepository : Repository<LmsModule>, ILmsModuleRepository
{
    public LmsModuleRepository(HRMSDbContext context) : base(context) { }

    public async Task<IEnumerable<LmsModule>> GetByCourseIdAsync(int courseId)
    {
        return await _dbSet
            .Where(m => m.LmsCourseId == courseId && m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
    }

    public async Task<LmsModule?> GetByIdAsync(int moduleId)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.LmsModuleId == moduleId);
    }
}
