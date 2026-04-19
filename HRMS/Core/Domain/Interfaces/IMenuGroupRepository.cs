using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IMenuGroupRepository : IRepository<MenuGroup>
{
    Task<IEnumerable<MenuGroup>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<IEnumerable<MenuGroup>> GetWithItemsAsync(int tenantId);
    Task<MenuGroup?> GetByIdAsync(int menuGroupId, int tenantId);
}
