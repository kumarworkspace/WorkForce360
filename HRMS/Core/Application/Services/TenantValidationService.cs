using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class TenantValidationService : ITenantValidationService
{
    private readonly ILogger<TenantValidationService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantRepository _tenantRepository;

    public TenantValidationService(
        ILogger<TenantValidationService> logger,
        IUnitOfWork unitOfWork,
        ITenantRepository tenantRepository)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _tenantRepository = tenantRepository;
    }

    public async Task<bool> IsValidTenantAsync(int tenantId)
    {
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId);
            return tenant != null && tenant.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> ValidateTenantAccessAsync(int userId, int tenantId)
    {
        try
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for tenant validation", userId);
                return false;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user {UserId} attempted tenant access", userId);
                return false;
            }

            if (user.TenantId != tenantId)
            {
                _logger.LogWarning("Tenant mismatch: User {UserId} belongs to tenant {UserTenantId}, requested tenant {RequestedTenantId}", 
                    userId, user.TenantId, tenantId);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tenant access for user {UserId}, tenant {TenantId}", userId, tenantId);
            return false;
        }
    }

    public async Task<bool> ValidateUserBelongsToTenantAsync(int userId, int tenantId)
    {
        try
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            return user != null && user.TenantId == tenantId && user.IsActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user belongs to tenant: User {UserId}, Tenant {TenantId}", userId, tenantId);
            return false;
        }
    }

    public void EnsureTenantMatch(int requestedTenantId, int userTenantId)
    {
        if (requestedTenantId != userTenantId)
        {
            _logger.LogWarning("Tenant mismatch detected: Requested {RequestedTenantId}, User Tenant {UserTenantId}", 
                requestedTenantId, userTenantId);
            throw new UnauthorizedAccessException($"Access denied: Tenant mismatch. You can only access data from your own tenant.");
        }
    }
}

