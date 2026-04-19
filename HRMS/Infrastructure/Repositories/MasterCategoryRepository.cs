using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class MasterCategoryRepository : Repository<MasterCategory>, IMasterCategoryRepository
{
    public MasterCategoryRepository(HRMSDbContext context) : base(context) { }

    public async Task<IEnumerable<MasterCategory>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        var query = _dbSet.Where(c => c.TenantId == tenantId);
        if (!includeInactive) query = query.Where(c => c.IsActive);
        return await query.OrderBy(c => c.CategoryName).ToListAsync();
    }

    public async Task<MasterCategory?> GetByCodeAsync(string categoryCode, int tenantId)
    {
        return await _dbSet.FirstOrDefaultAsync(c =>
            c.CategoryCode == categoryCode && c.TenantId == tenantId && c.IsActive);
    }

    public async Task<MasterCategory?> GetByIdAsync(int categoryId, int tenantId)
    {
        return await _dbSet.Include(c => c.MasterValues)
            .FirstOrDefaultAsync(c => c.MasterCategoryId == categoryId && c.TenantId == tenantId);
    }
}
