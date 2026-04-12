using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HRMS.Infrastructure.Middleware;

public class RoleBasedAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RoleBasedAuthorizationMiddleware> _logger;

    // Public pages that don't require authorization (beyond authentication)
    private static readonly HashSet<string> PublicPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/signin",
        "/terms-of-service",
        "/privacy-policy",
        "/dashboard",      // Dashboard accessible to all authenticated users
        "/tms/dashboard",  // TMS Dashboard accessible to all authenticated users
        "/profile",        // Every user can view/edit their own profile
    };

    // System paths that bypass authorization
    private static readonly HashSet<string> SystemPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/_host",
        "/_framework",
        "/css",
        "/js",
        "/images",
        "/login",
        "/login-handler",
        "/logout",
        "/MicrosoftIdentity",
        "/signin-oidc",
        "/signout-callback-oidc",
        "/_blazor",
        "/error",
        "/Certificates",
        "/uploads"
    };

    public RoleBasedAuthorizationMiddleware(RequestDelegate next, ILogger<RoleBasedAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAuthorizationHelper authorizationHelper,
        IUnitOfWorkFactory unitOfWorkFactory)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Allow system paths through without checks
        if (IsSystemPath(path))
        {
            await _next(context);
            return;
        }

        // Allow public paths through
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                await LogAccessAttemptAsync(unitOfWork.AuditLogs, context, path, false, "Not authenticated");
            }
            context.Response.Redirect("/signin?error=unauthorized");
            return;
        }

        // Get user info from claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                await LogAccessAttemptAsync(unitOfWork.AuditLogs, context, path, false, "Invalid user ID");
            }
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Access denied: Invalid user");
            return;
        }

        if (string.IsNullOrEmpty(tenantIdClaim) || !int.TryParse(tenantIdClaim, out var tenantId))
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                await LogAccessAttemptAsync(unitOfWork.AuditLogs, context, path, false, "Invalid tenant ID");
            }
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Access denied: Invalid tenant");
            return;
        }

        // Check page access using factory-created UnitOfWork to avoid DbContext concurrency
        bool hasAccess;
        using (var unitOfWork = unitOfWorkFactory.Create())
        {
            var moduleName = authorizationHelper.GetModuleNameFromPath(path);
            hasAccess = await CheckAccessAsync(unitOfWork, userId, moduleName, tenantId);
        }

        if (!hasAccess)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                await LogAccessAttemptAsync(unitOfWork.AuditLogs, context, path, false, "Insufficient permissions");
            }
            _logger.LogWarning("Access denied for user {UserId} to {Path} in tenant {TenantId}", userId, path, tenantId);
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Access denied: You do not have permission to access this page");
            return;
        }

        // Log successful access
        using (var unitOfWork = unitOfWorkFactory.Create())
        {
            await LogAccessAttemptAsync(unitOfWork.AuditLogs, context, path, true, "Access granted");
        }

        await _next(context);
    }

    private static bool IsPublicPath(string path)
    {
        return PublicPaths.Any(publicPath =>
            path.Equals(publicPath, StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(publicPath + "/", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSystemPath(string path)
    {
        return SystemPaths.Any(systemPath =>
            path.StartsWith(systemPath, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<bool> CheckAccessAsync(IUnitOfWork unitOfWork, int userId, string moduleName, int tenantId)
    {
        try
        {
            // Get user's roles
            var userRoles = await unitOfWork.UserRole.GetByUserIdAsync(userId, tenantId);
            if (!userRoles.Any()) return false;

            // Get permission for module
            var permission = await unitOfWork.Permission.GetByModuleNameAsync(moduleName, tenantId);
            if (permission == null) return false;

            // Get highest access level from all user's roles
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
            var rolePermissions = await unitOfWork.RolePermission.GetByPermissionIdAsync(permission.PermissionId, tenantId);
            
            var userRolePermissions = rolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId) && rp.IsActive)
                .ToList();

            return userRolePermissions.Any() && userRolePermissions.Max(rp => rp.AccessLevel) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking access for user: {UserId}, module: {ModuleName}, tenant: {TenantId}", userId, moduleName, tenantId);
            return false; // Deny access on error
        }
    }

    private async Task LogAccessAttemptAsync(
        IAuditLogRepository repository,
        HttpContext context,
        string path,
        bool success,
        string reason)
    {
        try
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;

            int? userId = null;
            int tenantId = 0;

            if (int.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            if (!int.TryParse(tenantIdClaim, out tenantId))
            {
                tenantId = 0; // Anonymous or invalid tenant
            }

            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                UserId = userId,
                ActionType = success ? ActionType.View : ActionType.Delete, // Use Delete for denied access
                Description = $"Page access: {path} - {(success ? "Granted" : "Denied")} - {reason}",
                IPAddress = GetClientIp(context),
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            await repository.AddAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log access attempt");
        }
    }

    private static string GetClientIp(HttpContext context)
    {
        // Check for forwarded IP (if behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        // Check for real IP
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fallback to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

