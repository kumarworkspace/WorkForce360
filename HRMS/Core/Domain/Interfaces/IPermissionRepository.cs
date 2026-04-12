using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByIdAsync(int permissionId, int tenantId);
    Task<IEnumerable<Permission>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<Permission?> GetByModuleNameAsync(string moduleName, int tenantId);
    Task<bool> ModuleNameExistsAsync(string moduleName, int tenantId, int? excludePermissionId = null);
    Task<bool> IsPermissionMappedToRolesAsync(int permissionId, int tenantId);
}





