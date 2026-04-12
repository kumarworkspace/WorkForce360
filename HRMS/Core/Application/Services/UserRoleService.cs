using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class UserRoleService : IUserRoleService
{
    private readonly ILogger<UserRoleService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UserRoleService(ILogger<UserRoleService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserRoleAssignmentDto> GetUserRoleAssignmentAsync(int userId, int tenantId)
    {
        try
        {
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            if (user == null || user.TenantId != tenantId)
            {
                throw new InvalidOperationException("User not found or access denied.");
            }

            var allRoles = await _unitOfWork.Role.GetByTenantIdAsync(tenantId);
            var userRoles = await _unitOfWork.UserRole.GetByUserIdAsync(userId, tenantId);
            var assignedRoleIds = userRoles.Select(ur => ur.RoleId).ToHashSet();

            var availableRoles = allRoles
                .Where(r => !assignedRoleIds.Contains(r.RoleId))
                .Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    TenantId = r.TenantId,
                    IsActive = r.IsActive
                }).ToList();

            var assignedRoles = allRoles
                .Where(r => assignedRoleIds.Contains(r.RoleId))
                .Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    TenantId = r.TenantId,
                    IsActive = r.IsActive
                }).ToList();

            return new UserRoleAssignmentDto
            {
                UserId = user.UserId,
                UserName = user.FullName,
                UserEmail = user.Email,
                RoleIds = assignedRoleIds.ToList(),
                AvailableRoles = availableRoles,
                AssignedRoles = assignedRoles
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user role assignment for user: {UserId}, tenant: {TenantId}", userId, tenantId);
            throw;
        }
    }

    public async Task<bool> AssignRolesAsync(AssignUserRolesRequest request, int tenantId, int userId)
    {
        try
        {
            var user = await _unitOfWork.User.GetByIdAsync(request.UserId);
            if (user == null || user.TenantId != tenantId)
            {
                throw new InvalidOperationException("User not found or access denied.");
            }

            // Validate all roles belong to tenant and are active
            var allRoles = await _unitOfWork.Role.GetByTenantIdAsync(tenantId);
            var validRoleIds = allRoles.Where(r => r.IsActive).Select(r => r.RoleId).ToHashSet();

            if (request.RoleIds.Any(roleId => !validRoleIds.Contains(roleId)))
            {
                throw new InvalidOperationException("One or more roles are invalid or inactive.");
            }

            // Get existing user roles
            var existingUserRoles = await _unitOfWork.UserRole.GetByUserIdAsync(request.UserId, tenantId);
            var existingRoleIds = existingUserRoles.Select(ur => ur.RoleId).ToHashSet();
            var requestedRoleIds = request.RoleIds.ToHashSet();

            // Remove roles that are no longer assigned
            var rolesToRemove = existingUserRoles.Where(ur => !requestedRoleIds.Contains(ur.RoleId)).ToList();
            foreach (var userRole in rolesToRemove)
            {
                userRole.IsActive = false;
                userRole.UpdatedBy = userId;
                userRole.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.UserRole.UpdateAsync(userRole);
            }

            // Add new roles
            var rolesToAdd = requestedRoleIds.Where(roleId => !existingRoleIds.Contains(roleId)).ToList();
            foreach (var roleId in rolesToAdd)
            {
                // Check if soft-deleted assignment exists
                var existing = await _unitOfWork.UserRole.GetByUserAndRoleAsync(request.UserId, roleId, tenantId);
                if (existing != null)
                {
                    existing.IsActive = true;
                    existing.UpdatedBy = userId;
                    existing.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.UserRole.UpdateAsync(existing);
                }
                else
                {
                    var userRole = new UserRole
                    {
                        UserId = request.UserId,
                        RoleId = roleId,
                        TenantId = tenantId,
                        IsActive = true,
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.UserRole.AddAsync(userRole);
                }
            }

            // Reactivate existing roles that were requested
            var rolesToReactivate = existingUserRoles
                .Where(ur => requestedRoleIds.Contains(ur.RoleId) && !ur.IsActive)
                .ToList();
            foreach (var userRole in rolesToReactivate)
            {
                userRole.IsActive = true;
                userRole.UpdatedBy = userId;
                userRole.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.UserRole.UpdateAsync(userRole);
            }

            await _unitOfWork.SaveChangesAsync();

            // Log audit
            var targetUser = await _unitOfWork.User.GetByIdAsync(request.UserId);
            var assignedRoleNames = string.Join(", ", allRoles.Where(r => requestedRoleIds.Contains(r.RoleId)).Select(r => r.RoleName));
            await LogAuditAsync(tenantId, userId, ActionType.Update, 
                $"Assigned roles to user: {targetUser?.FullName ?? "Unknown"} (ID: {request.UserId}). Roles: {assignedRoleNames}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user: {UserId} in tenant: {TenantId}", request.UserId, tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<UserRoleDto>> GetByUserIdAsync(int userId, int tenantId)
    {
        try
        {
            var userRoles = await _unitOfWork.UserRole.GetByUserIdAsync(userId, tenantId);
            var user = await _unitOfWork.User.GetByIdAsync(userId);
            
            return userRoles.Select(ur => new UserRoleDto
            {
                Id = ur.Id,
                UserId = userId,
                UserName = user?.FullName ?? string.Empty,
                UserEmail = user?.Email ?? string.Empty,
                RoleId = ur.RoleId,
                RoleName = ur.Role?.RoleName ?? string.Empty,
                TenantId = ur.TenantId,
                IsActive = ur.IsActive,
                CreatedDate = ur.CreatedDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles for user: {UserId}, tenant: {TenantId}", userId, tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<UserRoleDto>> GetByRoleIdAsync(int roleId, int tenantId)
    {
        try
        {
            var userRoles = await _unitOfWork.UserRole.GetByRoleIdAsync(roleId, tenantId);
            var userIds = userRoles.Select(ur => ur.UserId).Distinct().ToList();
            
            // Get user details for all user IDs
            var users = new Dictionary<int, User>();
            foreach (var userId in userIds)
            {
                var user = await _unitOfWork.User.GetByIdAsync(userId);
                if (user != null)
                {
                    users[userId] = user;
                }
            }
            
            return userRoles.Select(ur => 
            {
                var user = users.ContainsKey(ur.UserId) ? users[ur.UserId] : null;
                return new UserRoleDto
                {
                    Id = ur.Id,
                    UserId = ur.UserId,
                    UserName = user?.FullName ?? string.Empty,
                    UserEmail = user?.Email ?? string.Empty,
                    RoleId = ur.RoleId,
                    RoleName = ur.Role?.RoleName ?? string.Empty,
                    TenantId = ur.TenantId,
                    IsActive = ur.IsActive,
                    CreatedDate = ur.CreatedDate
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users for role: {RoleId}, tenant: {TenantId}", roleId, tenantId);
            throw;
        }
    }

    public async Task<bool> RemoveRoleAsync(int userId, int roleId, int tenantId, int currentUserId)
    {
        try
        {
            var userRole = await _unitOfWork.UserRole.GetByUserAndRoleAsync(userId, roleId, tenantId);
            if (userRole == null) return false;

            userRole.IsActive = false;
            userRole.UpdatedBy = currentUserId;
            userRole.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.UserRole.UpdateAsync(userRole);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            var targetUser = await _unitOfWork.User.GetByIdAsync(userId);
            var role = await _unitOfWork.Role.GetByIdAsync(roleId, tenantId);
            await LogAuditAsync(tenantId, currentUserId, ActionType.Update, 
                $"Removed role '{role?.RoleName ?? "Unknown"}' from user: {targetUser?.FullName ?? "Unknown"} (ID: {userId})");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user: {UserId}, role: {RoleId}, tenant: {TenantId}", userId, roleId, tenantId);
            throw;
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
            _logger.LogWarning(ex, "Failed to log audit entry for user role operation");
            // Don't throw - audit logging failure shouldn't break the operation
        }
    }

}

