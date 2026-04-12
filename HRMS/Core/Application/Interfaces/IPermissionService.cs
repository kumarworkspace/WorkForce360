using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IPermissionService
{
    Task<PermissionDto> CreateAsync(CreatePermissionRequest request, int tenantId, int userId);
    Task<PermissionDto> UpdateAsync(UpdatePermissionRequest request, int tenantId, int userId);
    Task<bool> DeleteAsync(int permissionId, int tenantId, int userId);
    Task<PermissionDto?> GetByIdAsync(int permissionId, int tenantId);
    Task<IEnumerable<PermissionDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<bool> ActivateAsync(int permissionId, int tenantId, int userId);
    Task<bool> DeactivateAsync(int permissionId, int tenantId, int userId);
}





