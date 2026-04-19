using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IMenuItemRepository : IRepository<MenuItem>
{
    Task<IEnumerable<MenuItem>> GetByGroupIdAsync(int menuGroupId, int tenantId);
    Task<MenuItem?> GetByIdAsync(int menuItemId, int tenantId);
}
