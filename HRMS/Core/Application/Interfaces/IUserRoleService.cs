using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IUserRoleService
{
    Task<UserRoleAssignmentDto> GetUserRoleAssignmentAsync(int userId, int tenantId);
    Task<bool> AssignRolesAsync(AssignUserRolesRequest request, int tenantId, int userId);
    Task<IEnumerable<UserRoleDto>> GetByUserIdAsync(int userId, int tenantId);
    Task<IEnumerable<UserRoleDto>> GetByRoleIdAsync(int roleId, int tenantId);
    Task<bool> RemoveRoleAsync(int userId, int roleId, int tenantId, int currentUserId);
}





