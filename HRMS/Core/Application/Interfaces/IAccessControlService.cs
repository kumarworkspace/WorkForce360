using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IAccessControlService
{
    Task<AccessControlMatrixDto> GetAccessControlMatrixAsync(int roleId, int tenantId);
    Task<bool> UpdateAccessControlAsync(UpdateAccessControlRequest request, int tenantId, int userId);
    Task<int> GetAccessLevelAsync(int userId, string moduleName, int tenantId);
    Task<bool> HasAccessAsync(int userId, string moduleName, int requiredAccessLevel, int tenantId);
    Task<Dictionary<string, int>> GetUserPermissionsAsync(int userId, int tenantId);
}





