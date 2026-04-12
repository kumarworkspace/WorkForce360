using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class AccessControlService : IAccessControlService
{
    private readonly ILogger<AccessControlService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AccessControlService(ILogger<AccessControlService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccessControlMatrixDto> GetAccessControlMatrixAsync(int roleId, int tenantId)
    {
        try
        {
            var role = await _unitOfWork.Role.GetByIdAsync(roleId, tenantId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found or access denied.");
            }

            var permissions = await _unitOfWork.Permission.GetByTenantIdAsync(tenantId);
            var rolePermissions = await _unitOfWork.RolePermission.GetByRoleIdAsync(roleId, tenantId);

            var permissionAccessList = permissions.Select(p => new PermissionAccessDto
            {
                PermissionId = p.PermissionId,
                ModuleName = p.ModuleName,
                AccessLevel = rolePermissions.FirstOrDefault(rp => rp.PermissionId == p.PermissionId)?.AccessLevel ?? 0
            }).ToList();

            return new AccessControlMatrixDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Permissions = permissionAccessList
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access control matrix for role: {RoleId} in tenant: {TenantId}", roleId, tenantId);
            throw;
        }
    }

    public async Task<bool> UpdateAccessControlAsync(UpdateAccessControlRequest request, int tenantId, int userId)
    {
        try
        {
            var role = await _unitOfWork.Role.GetByIdAsync(request.RoleId, tenantId);
            if (role == null)
            {
                throw new InvalidOperationException("Role not found or access denied.");
            }

            // Validate all permissions belong to tenant
            var permissionIds = request.Permissions.Select(p => p.PermissionId).ToList();
            var permissions = await _unitOfWork.Permission.GetByTenantIdAsync(tenantId);
            var validPermissionIds = permissions.Select(p => p.PermissionId).ToList();

            if (permissionIds.Any(id => !validPermissionIds.Contains(id)))
            {
                throw new InvalidOperationException("One or more permissions do not belong to this tenant.");
            }

            // Validate access levels (0-3)
            if (request.Permissions.Any(p => p.AccessLevel < 0 || p.AccessLevel > 3))
            {
                throw new InvalidOperationException("Access level must be between 0 and 3.");
            }

            // Get existing role permissions
            var existingRolePermissions = await _unitOfWork.RolePermission.GetByRoleIdAsync(request.RoleId, tenantId);
            var existingDict = existingRolePermissions.ToDictionary(rp => rp.PermissionId);

            // Update or create role permissions
            foreach (var permissionAccess in request.Permissions)
            {
                if (existingDict.TryGetValue(permissionAccess.PermissionId, out var existing))
                {
                    // Update existing
                    existing.AccessLevel = permissionAccess.AccessLevel;
                    existing.IsActive = true;
                    existing.UpdatedBy = userId;
                    existing.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.RolePermission.UpdateAsync(existing);
                }
                else if (permissionAccess.AccessLevel > 0)
                {
                    // Create new only if access level > 0
                    var rolePermission = new RolePermission
                    {
                        RoleId = request.RoleId,
                        PermissionId = permissionAccess.PermissionId,
                        AccessLevel = permissionAccess.AccessLevel,
                        TenantId = tenantId,
                        IsActive = true,
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.RolePermission.AddAsync(rolePermission);
                }
            }

            // Soft delete permissions that are no longer in the request (set to 0)
            var requestPermissionIds = request.Permissions.Select(p => p.PermissionId).ToHashSet();
            foreach (var existing in existingRolePermissions)
            {
                if (!requestPermissionIds.Contains(existing.PermissionId))
                {
                    existing.IsActive = false;
                    existing.UpdatedBy = userId;
                    existing.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.RolePermission.UpdateAsync(existing);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Log audit
            var roleForAudit = await _unitOfWork.Role.GetByIdAsync(request.RoleId, tenantId);
            var permissionNames = string.Join(", ", request.Permissions.Select(p => 
            {
                var perm = permissions.FirstOrDefault(perm => perm.PermissionId == p.PermissionId);
                return perm != null ? $"{perm.ModuleName}({p.AccessLevel})" : $"PermissionId:{p.PermissionId}({p.AccessLevel})";
            }));
            await LogAuditAsync(tenantId, userId, ActionType.Update, 
                $"Updated access control for role: {roleForAudit?.RoleName ?? "Unknown"} (ID: {request.RoleId}). Permissions: {permissionNames}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating access control for role: {RoleId} in tenant: {TenantId}", request.RoleId, tenantId);
            throw;
        }
    }

    public async Task<int> GetAccessLevelAsync(int userId, string moduleName, int tenantId)
    {
        try
        {
            // Get user's roles
            var userRoles = await _unitOfWork.UserRole.GetByUserIdAsync(userId, tenantId);
            if (!userRoles.Any()) return 0;

            // Get permission for module
            var permission = await _unitOfWork.Permission.GetByModuleNameAsync(moduleName, tenantId);
            if (permission == null) return 0;

            // Get highest access level from all user's roles
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var rolePermissions = await _unitOfWork.RolePermission.GetByPermissionIdAsync(permission.PermissionId, tenantId);
            
            var userRolePermissions = rolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive)
                .ToList();

            return userRolePermissions.Any() ? userRolePermissions.Max(rp => rp.AccessLevel) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access level for user: {UserId}, module: {ModuleName}, tenant: {TenantId}", userId, moduleName, tenantId);
            return 0;
        }
    }

    public async Task<bool> HasAccessAsync(int userId, string moduleName, int requiredAccessLevel, int tenantId)
    {
        var accessLevel = await GetAccessLevelAsync(userId, moduleName, tenantId);
        return accessLevel >= requiredAccessLevel;
    }

    public async Task<Dictionary<string, int>> GetUserPermissionsAsync(int userId, int tenantId)
    {
        try
        {
            // Get user's roles
            var userRoles = await _unitOfWork.UserRole.GetByUserIdAsync(userId, tenantId);
            if (!userRoles.Any()) return new Dictionary<string, int>();

            // Get all permissions for tenant
            var permissions = await _unitOfWork.Permission.GetByTenantIdAsync(tenantId);
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            var result = new Dictionary<string, int>();

            foreach (var permission in permissions)
            {
                var rolePermissions = await _unitOfWork.RolePermission.GetByPermissionIdAsync(permission.PermissionId, tenantId);
                var userRolePermissions = rolePermissions
                    .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive)
                    .ToList();

                var maxAccessLevel = userRolePermissions.Any() ? userRolePermissions.Max(rp => rp.AccessLevel) : 0;
                result[permission.ModuleName] = maxAccessLevel;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user permissions for user: {UserId}, tenant: {TenantId}", userId, tenantId);
            return new Dictionary<string, int>();
        }
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
            _logger.LogWarning(ex, "Failed to log audit entry for access control operation");
            // Don't throw - audit logging failure shouldn't break the operation
        }
    }

}

