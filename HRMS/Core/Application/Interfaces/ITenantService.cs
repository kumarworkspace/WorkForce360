using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Application.Interfaces;

public interface ITenantService
{
    Task<IEnumerable<Tenant>> GetAllTenantsAsync();
    Task<Tenant?> GetTenantByIdAsync(int tenantId);
    Task<Tenant?> GetTenantByNameAsync(string companyName);
    Task<Tenant> CreateTenantAsync(Tenant tenant);
    Task UpdateTenantAsync(Tenant tenant);
    Task<bool> DeleteTenantAsync(int tenantId);
    Task<bool> TenantNameExistsAsync(string companyName);
    Task LockTenantAsync(int tenantId);
    Task UnlockTenantAsync(int tenantId);
    Task ActivateTenantAsync(int tenantId);
    Task DeactivateTenantAsync(int tenantId);
}


