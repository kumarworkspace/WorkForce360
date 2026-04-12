using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByIdAsync(int roleId, int tenantId);
    Task<IEnumerable<Role>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<bool> RoleNameExistsAsync(string roleName, int tenantId, int? excludeRoleId = null);
    Task<bool> IsRoleAssignedToUsersAsync(int roleId, int tenantId);
}





