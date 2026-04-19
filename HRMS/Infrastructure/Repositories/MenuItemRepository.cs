using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class MenuItemRepository : Repository<MenuItem>, IMenuItemRepository
{
    public MenuItemRepository(HRMSDbContext context) : base(context) { }

    public async Task<IEnumerable<MenuItem>> GetByGroupIdAsync(int menuGroupId, int tenantId)
    {
        return await _dbSet
            .Where(i => i.MenuGroupId == menuGroupId && i.TenantId == tenantId && i.IsActive)
            .OrderBy(i => i.SortOrder)
            .ToListAsync();
    }

    public async Task<MenuItem?> GetByIdAsync(int menuItemId, int tenantId)
    {
        return await _dbSet.FirstOrDefaultAsync(i => i.MenuItemId == menuItemId && i.TenantId == tenantId);
    }
}
