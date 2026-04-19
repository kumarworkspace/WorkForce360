using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class MasterValueRepository : Repository<MasterValue>, IMasterValueRepository
{
    public MasterValueRepository(HRMSDbContext context) : base(context) { }

    public async Task<IEnumerable<MasterValue>> GetByCategoryIdAsync(int categoryId, bool includeInactive = false)
    {
        var query = _dbSet.Where(v => v.MasterCategoryId == categoryId);
        if (!includeInactive) query = query.Where(v => v.IsActive);
        return await query.OrderBy(v => v.SortOrder).ThenBy(v => v.ValueName).ToListAsync();
    }

    public async Task<IEnumerable<MasterValue>> GetByCategoryCodeAsync(string categoryCode, int tenantId)
    {
        return await _dbSet
            .Include(v => v.MasterCategory)
            .Where(v => v.MasterCategory.CategoryCode == categoryCode
                     && v.TenantId == tenantId
                     && v.IsActive)
            .OrderBy(v => v.SortOrder).ThenBy(v => v.ValueName)
            .ToListAsync();
    }

    public async Task<MasterValue?> GetByIdAsync(int valueId, int tenantId)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.MasterValueId == valueId && v.TenantId == tenantId);
    }
}
