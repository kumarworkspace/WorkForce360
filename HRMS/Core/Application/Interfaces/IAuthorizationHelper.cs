namespace HRMS.Core.Application.Interfaces;

public interface IAuthorizationHelper
{
    Task<bool> HasPageAccessAsync(int userId, string pagePath, int tenantId);
    Task<int> GetPageAccessLevelAsync(int userId, string pagePath, int tenantId);
    Task<int> GetModuleAccessLevelAsync(int userId, string moduleName, int tenantId);
    Task<Dictionary<string, int>> GetUserPagePermissionsAsync(int userId, int tenantId);
    string GetModuleNameFromPath(string path);
}

