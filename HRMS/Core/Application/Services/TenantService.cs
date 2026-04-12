using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class TenantService : ITenantService
{
    private readonly ILogger<TenantService> _logger;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TenantService(
        ILogger<TenantService> logger,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
    {
        return await _tenantRepository.GetAllAsync();
    }

    public async Task<Tenant?> GetTenantByIdAsync(int tenantId)
    {
        return await _tenantRepository.GetByIdAsync(tenantId);
    }

    public async Task<Tenant?> GetTenantByNameAsync(string companyName)
    {
        return await _tenantRepository.GetByNameAsync(companyName);
    }

    public async Task<Tenant> CreateTenantAsync(Tenant tenant)
    {
        if (await _tenantRepository.TenantNameExistsAsync(tenant.CompanyName))
        {
            throw new InvalidOperationException($"Company with name '{tenant.CompanyName}' already exists.");
        }

        await _tenantRepository.AddAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Tenant created: {CompanyName} (ID: {TenantId})", tenant.CompanyName, tenant.TenantId);
        return tenant;
    }

    public async Task UpdateTenantAsync(Tenant tenant)
    {
        var existingTenant = await _tenantRepository.GetByIdAsync(tenant.TenantId);
        if (existingTenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID {tenant.TenantId} not found.");
        }

        // Check if name is being changed and if new name already exists
        if (existingTenant.CompanyName != tenant.CompanyName && await _tenantRepository.TenantNameExistsAsync(tenant.CompanyName))
        {
            throw new InvalidOperationException($"Company with name '{tenant.CompanyName}' already exists.");
        }

        await _tenantRepository.UpdateAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Tenant updated: {CompanyName} (ID: {TenantId})", tenant.CompanyName, tenant.TenantId);
    }

    public async Task<bool> DeleteTenantAsync(int tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            return false;
        }

        await _tenantRepository.DeleteAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Tenant deleted: {TenantId}", tenantId);
        return true;
    }

    public async Task<bool> TenantNameExistsAsync(string companyName)
    {
        return await _tenantRepository.TenantNameExistsAsync(companyName);
    }

    public async Task LockTenantAsync(int tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found.");
        }

        tenant.IsLocked = true;
        await _tenantRepository.UpdateAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Tenant locked: {TenantId}", tenantId);
    }

    public async Task UnlockTenantAsync(int tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found.");
        }

        tenant.IsLocked = false;
        await _tenantRepository.UpdateAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Tenant unlocked: {TenantId}", tenantId);
    }

    public async Task ActivateTenantAsync(int tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found.");
        }

        tenant.IsActive = true;
        await _tenantRepository.UpdateAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Tenant activated: {TenantId}", tenantId);
    }

    public async Task DeactivateTenantAsync(int tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with ID {tenantId} not found.");
        }

        tenant.IsActive = false;
        await _tenantRepository.UpdateAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Tenant deactivated: {TenantId}", tenantId);
    }
}


