namespace HRMS.Core.Application.Interfaces;

public interface ITenantValidationService
{
    Task<bool> IsValidTenantAsync(int tenantId);
    Task<bool> ValidateTenantAccessAsync(int userId, int tenantId);
    Task<bool> ValidateUserBelongsToTenantAsync(int userId, int tenantId);
    void EnsureTenantMatch(int requestedTenantId, int userTenantId);
}

