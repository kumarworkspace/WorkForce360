using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IRoleService
{
    Task<RoleDto> CreateAsync(CreateRoleRequest request, int tenantId, int userId);
    Task<RoleDto> UpdateAsync(UpdateRoleRequest request, int tenantId, int userId);
    Task<bool> DeleteAsync(int roleId, int tenantId, int userId);
    Task<RoleDto?> GetByIdAsync(int roleId, int tenantId);
    Task<IEnumerable<RoleDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<bool> ActivateAsync(int roleId, int tenantId, int userId);
    Task<bool> DeactivateAsync(int roleId, int tenantId, int userId);
}





