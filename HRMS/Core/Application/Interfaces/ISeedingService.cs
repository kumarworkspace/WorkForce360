namespace HRMS.Core.Application.Interfaces;

public interface ISeedingService
{
    Task SeedDefaultRolesAndPermissionsAsync(int tenantId, int createdByUserId);
    Task<bool> HasRolesAndPermissionsAsync(int tenantId);
    Task EnsureRolePermissionsSeededAsync(int tenantId, int createdByUserId);
    Task EnsureMissingPermissionsAsync(int tenantId, int createdByUserId);
}





