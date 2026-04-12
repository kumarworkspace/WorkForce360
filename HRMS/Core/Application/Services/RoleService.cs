using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class RoleService : IRoleService
{
    private readonly ILogger<RoleService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(ILogger<RoleService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request, int tenantId, int userId)
    {
        try
        {
            // Tenant validation: Ensure user belongs to the tenant
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null || user.TenantId != tenantId)
            {
                throw new UnauthorizedAccessException("Access denied: You can only create roles for your own tenant.");
            }

            // Validate role name uniqueness
            if (await _unitOfWork.Role.RoleNameExistsAsync(request.RoleName, tenantId))
            {
                throw new InvalidOperationException($"Role name '{request.RoleName}' already exists for this tenant.");
            }

            var role = new Role
            {
                RoleName = request.RoleName.Trim(),
                Description = request.Description?.Trim(),
                TenantId = tenantId,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Role.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Create, $"Created role: {role.RoleName} (ID: {role.RoleId})");

            return MapToDto(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<RoleDto> UpdateAsync(UpdateRoleRequest request, int tenantId, int userId)
    {
        try
        {
            var role = await _unitOfWork.Role.GetByIdAsync(request.RoleId, tenantId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found or access denied.");
            }

            // Validate role name uniqueness (exclude current role)
            if (await _unitOfWork.Role.RoleNameExistsAsync(request.RoleName, tenantId, request.RoleId))
            {
                throw new InvalidOperationException($"Role name '{request.RoleName}' already exists for this tenant.");
            }

            role.RoleName = request.RoleName.Trim();
            role.Description = request.Description?.Trim();
            role.IsActive = request.IsActive;
            role.UpdatedBy = userId;
            role.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Role.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Update, $"Updated role: {role.RoleName} (ID: {role.RoleId})");

            return MapToDto(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId} for tenant: {TenantId}", request.RoleId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int roleId, int tenantId, int userId)
    {
        try
        {
            var role = await _unitOfWork.Role.GetByIdAsync(roleId, tenantId);
            if (role == null)
            {
                return false;
            }

            // Check if role is assigned to users
            if (await _unitOfWork.Role.IsRoleAssignedToUsersAsync(roleId, tenantId))
            {
                throw new InvalidOperationException("Cannot delete role that is assigned to users. Please remove all user assignments first.");
            }

            var roleName = role.RoleName;
            await _unitOfWork.Role.DeleteAsync(role);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Delete, $"Deleted role: {roleName} (ID: {roleId})");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId} for tenant: {TenantId}", roleId, tenantId);
            throw;
        }
    }

    public async Task<RoleDto?> GetByIdAsync(int roleId, int tenantId)
    {
        try
        {
            var role = await _unitOfWork.Role.GetByIdAsync(roleId, tenantId);
            if (role == null) return null;

            var dto = MapToDto(role);
            var userRoles = await _unitOfWork.UserRole.GetByRoleIdAsync(roleId, tenantId);
            dto.UserCount = userRoles.Count();

            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role: {RoleId} for tenant: {TenantId}", roleId, tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<RoleDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        try
        {
            var roles = await _unitOfWork.Role.GetByTenantIdAsync(tenantId, includeInactive);
            var roleDtos = roles.Select(r => MapToDto(r)).ToList();

            // Get user counts for each role (without loading User navigation to avoid shadow property issues)
            foreach (var dto in roleDtos)
            {
                try
                {
                    var userRoles = await _unitOfWork.UserRole.GetByRoleIdAsync(dto.RoleId, tenantId);
                    dto.UserCount = userRoles.Count();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting user count for role {RoleId}, defaulting to 0", dto.RoleId);
                    dto.UserCount = 0; // Default to 0 if there's an error
                }
            }

            return roleDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> ActivateAsync(int roleId, int tenantId, int userId)
    {
        try
        {
            var role = await _unitOfWork.Role.GetByIdAsync(roleId, tenantId);
            if (role == null) return false;

            role.IsActive = true;
            role.UpdatedBy = userId;
            role.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Role.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Update, $"Activated role: {role.RoleName} (ID: {roleId})");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating role: {RoleId} for tenant: {TenantId}", roleId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeactivateAsync(int roleId, int tenantId, int userId)
    {
        try
        {
            var role = await _unitOfWork.Role.GetByIdAsync(roleId, tenantId);
            if (role == null) return false;

            role.IsActive = false;
            role.UpdatedBy = userId;
            role.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Role.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Update, $"Deactivated role: {role.RoleName} (ID: {roleId})");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating role: {RoleId} for tenant: {TenantId}", roleId, tenantId);
            throw;
        }
    }

    private RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description,
            TenantId = role.TenantId,
            IsActive = role.IsActive,
            CreatedDate = role.CreatedDate,
            CreatedBy = role.CreatedBy,
            UpdatedDate = role.UpdatedDate,
            UpdatedBy = role.UpdatedBy
        };
    }

    private async Task LogAuditAsync(int tenantId, int userId, ActionType actionType, string description)
    {
        try
        {
            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                UserId = userId,
                ActionType = actionType,
                Description = description,
                IPAddress = null, // IP address not available in service layer
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log audit entry for role operation");
            // Don't throw - audit logging failure shouldn't break the operation
        }
    }
}

