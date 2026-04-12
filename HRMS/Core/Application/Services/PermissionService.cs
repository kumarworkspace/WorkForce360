using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly ILogger<PermissionService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(ILogger<PermissionService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<PermissionDto> CreateAsync(CreatePermissionRequest request, int tenantId, int userId)
    {
        try
        {
            // Validate module name uniqueness
            if (await _unitOfWork.Permission.ModuleNameExistsAsync(request.ModuleName, tenantId))
            {
                throw new InvalidOperationException($"Module name '{request.ModuleName}' already exists for this tenant.");
            }

            var permission = new Permission
            {
                ModuleName = request.ModuleName.Trim(),
                TenantId = tenantId,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Permission.AddAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<PermissionDto> UpdateAsync(UpdatePermissionRequest request, int tenantId, int userId)
    {
        try
        {
            var permission = await _unitOfWork.Permission.GetByIdAsync(request.PermissionId, tenantId);
            if (permission == null)
            {
                throw new InvalidOperationException("Permission not found or access denied.");
            }

            // Validate module name uniqueness (exclude current permission)
            if (await _unitOfWork.Permission.ModuleNameExistsAsync(request.ModuleName, tenantId, request.PermissionId))
            {
                throw new InvalidOperationException($"Module name '{request.ModuleName}' already exists for this tenant.");
            }

            permission.ModuleName = request.ModuleName.Trim();
            permission.IsActive = request.IsActive;
            permission.UpdatedBy = userId;
            permission.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Permission.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permission: {PermissionId} for tenant: {TenantId}", request.PermissionId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int permissionId, int tenantId, int userId)
    {
        try
        {
            var permission = await _unitOfWork.Permission.GetByIdAsync(permissionId, tenantId);
            if (permission == null)
            {
                return false;
            }

            // Check if permission is mapped to roles
            if (await _unitOfWork.Permission.IsPermissionMappedToRolesAsync(permissionId, tenantId))
            {
                throw new InvalidOperationException("Cannot delete permission that is mapped to roles. Please remove all role mappings first.");
            }

            await _unitOfWork.Permission.DeleteAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting permission: {PermissionId} for tenant: {TenantId}", permissionId, tenantId);
            throw;
        }
    }

    public async Task<PermissionDto?> GetByIdAsync(int permissionId, int tenantId)
    {
        try
        {
            var permission = await _unitOfWork.Permission.GetByIdAsync(permissionId, tenantId);
            if (permission == null) return null;

            return MapToDto(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permission: {PermissionId} for tenant: {TenantId}", permissionId, tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<PermissionDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        try
        {
            var permissions = await _unitOfWork.Permission.GetByTenantIdAsync(tenantId, includeInactive);
            return permissions.Select(p => MapToDto(p));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> ActivateAsync(int permissionId, int tenantId, int userId)
    {
        try
        {
            var permission = await _unitOfWork.Permission.GetByIdAsync(permissionId, tenantId);
            if (permission == null) return false;

            permission.IsActive = true;
            permission.UpdatedBy = userId;
            permission.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Permission.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating permission: {PermissionId} for tenant: {TenantId}", permissionId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeactivateAsync(int permissionId, int tenantId, int userId)
    {
        try
        {
            var permission = await _unitOfWork.Permission.GetByIdAsync(permissionId, tenantId);
            if (permission == null) return false;

            permission.IsActive = false;
            permission.UpdatedBy = userId;
            permission.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Permission.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating permission: {PermissionId} for tenant: {TenantId}", permissionId, tenantId);
            throw;
        }
    }

    private static PermissionDto MapToDto(Permission permission)
    {
        return new PermissionDto
        {
            PermissionId = permission.PermissionId,
            ModuleName = permission.ModuleName,
            TenantId = permission.TenantId,
            IsActive = permission.IsActive,
            CreatedDate = permission.CreatedDate,
            CreatedBy = permission.CreatedBy
        };
    }
}





