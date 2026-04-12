using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IRolePermissionRepository : IRepository<RolePermission>
{
    Task<RolePermission?> GetByRoleAndPermissionAsync(int roleId, int permissionId, int tenantId);
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId, int tenantId);
    Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(int permissionId, int tenantId);
    Task<bool> ExistsAsync(int roleId, int permissionId, int tenantId);
    Task DeleteByRoleIdAsync(int roleId, int tenantId);
}





