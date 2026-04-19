using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class MenuGroupRepository : Repository<MenuGroup>, IMenuGroupRepository
{
    public MenuGroupRepository(HRMSDbContext context) : base(context) { }

    public async Task<IEnumerable<MenuGroup>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        var query = _dbSet.Where(g => g.TenantId == tenantId);
        if (!includeInactive) query = query.Where(g => g.IsActive);
        return await query.OrderBy(g => g.SortOrder).ToListAsync();
    }

    public async Task<IEnumerable<MenuGroup>> GetWithItemsAsync(int tenantId)
    {
        return await _dbSet
            .Where(g => g.TenantId == tenantId && g.IsActive)
            .Include(g => g.MenuItems.Where(i => i.IsActive && i.TenantId == tenantId))
            .OrderBy(g => g.SortOrder)
            .ToListAsync();
    }

    public async Task<MenuGroup?> GetByIdAsync(int menuGroupId, int tenantId)
    {
        return await _dbSet
            .Include(g => g.MenuItems.Where(i => i.TenantId == tenantId))
            .FirstOrDefaultAsync(g => g.MenuGroupId == menuGroupId && g.TenantId == tenantId);
    }
}
