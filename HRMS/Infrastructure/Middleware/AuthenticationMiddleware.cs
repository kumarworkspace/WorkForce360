using Microsoft.AspNetCore.Http;
using HRMS.Core.Domain.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;

namespace HRMS.Infrastructure.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    // Public pages that don't require authentication
    private static readonly HashSet<string> PublicPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/signin",
        "/terms-of-service",
        "/privacy-policy"
    };

    // Framework and authentication paths that should be allowed
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
        "/_blazor"
    };

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWorkFactory unitOfWorkFactory)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Allow system/framework paths through without any checks
        if (IsSystemPath(path))
        {
            await _next(context);
            return;
        }

        // Allow public paths through without authentication
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // For root path, redirect to signin if not authenticated, otherwise to dashboard
        if (path == "/")
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                context.Response.Redirect("/signin");
                return;
            }
            else
            {
                context.Response.Redirect("/dashboard");
                return;
            }
        }

        // Check authentication for all other protected paths
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            // Log unauthorized access attempt using factory-created UnitOfWork
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                await LogAuditAsync(unitOfWork.AuditLogs, context, ActionType.View, "Unauthorized access attempt");
                await unitOfWork.SaveChangesAsync();
            }

            _logger.LogWarning("Unauthorized access attempt to {Path} from {IP}", path, GetClientIp(context));
            context.Response.Redirect("/signin");
            return;
        }

        // Log successful access using factory-created UnitOfWork
        using (var unitOfWork = unitOfWorkFactory.Create())
        {
            await LogAuditAsync(unitOfWork.AuditLogs, context, ActionType.View, $"Accessed {path}");
            await unitOfWork.SaveChangesAsync();
        }

        await _next(context);
    }

    private static bool IsPublicPath(string path)
    {
        // Check exact match for public paths
        return PublicPaths.Any(publicPath =>
            path.Equals(publicPath, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSystemPath(string path)
    {
        // Check if path starts with any system path
        return SystemPaths.Any(systemPath =>
            path.StartsWith(systemPath, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetClientIp(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static string GetUserAgent(HttpContext context)
    {
        return context.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }

    private async Task LogAuditAsync(IAuditLogRepository repository, HttpContext context, ActionType actionType, string details)
    {
        try
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            int? userId = null;
            if (int.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            int tenantId = 0;
            if (!int.TryParse(tenantIdClaim, out tenantId))
            {
                tenantId = 0; // Log with tenant 0 for anonymous/unauthenticated access
            }

            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                UserId = userId,
                ActionType = actionType,
                Description = $"{details} - Path: {context.Request.Path.Value} - Role: {roleClaim ?? "N/A"} - Method: {context.Request.Method}",
                IPAddress = GetClientIp(context),
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            await repository.AddAsync(auditLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry");
        }
    }
}
