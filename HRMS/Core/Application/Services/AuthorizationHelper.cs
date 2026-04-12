using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class AuthorizationHelper : IAuthorizationHelper
{
    private readonly ILogger<AuthorizationHelper> _logger;
    private readonly IAccessControlService _accessControlService;
    private readonly IUnitOfWork _unitOfWork;

    // Map page paths to Permission.ModuleName values in the database.
    // This is URL routing config — it defines which permission controls each page.
    // Values must exactly match CanonicalPermissions in SeedingService.
    private static readonly Dictionary<string, string> PageToModuleMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Employee Management
        { "/hrms/employees",             "Employee Management" },
        { "/hrms/employees/form",        "Employee Management" },
        { "/hrms/employees/view",        "Employee Management" },
        { "/hrms/attendance",            "Employee Management" },

        // Leave — own leave requests (all staff)
        { "/hrms/leave",                 "Leave Management" },
        { "/hrms/leave/request",         "Leave Management" },
        { "/hrms/leave/edit",            "Leave Management" },
        { "/hrms/myleave",               "Leave Management" },

        // Leave Approval — managers/HR only
        { "/hrms/leave/approval",        "Leave Approval" },

        // Leave Configuration — HR/admin only
        { "/hrms/leave/types",           "Leave Configuration" },
        { "/hrms/leave/holidays",        "Leave Configuration" },

        // My Training — staff-facing my courses page
        { "/tms/my-courses",             "My Training" },
        { "/tms/my-planning",            "My Training" },

        // Course Management — trainer/admin TMS pages
        { "/tms/courses",                "Course Management" },
        { "/tms/courses/create",         "Course Management" },
        { "/tms/courses/edit",           "Course Management" },
        { "/tms/planning",               "Course Management" },
        { "/tms/planning/create",        "Course Management" },
        { "/tms/planning/view",          "Course Management" },
        { "/tms/planning/edit",          "Course Management" },
        { "/tms/attendance",             "Course Management" },
        { "/tms/reports",               "Course Management" },

        // Admin
        { "/admin/users",                "Role Management" },
        { "/admin/create-user",          "Role Management" },
        { "/admin/roles",                "Role Management" },
        { "/admin/access-control",       "Access Control" },
        { "/admin/user-roles",           "Access Control" },
        { "/admin/audit-logs",           "Audit Logs" },
    };

    public AuthorizationHelper(
        ILogger<AuthorizationHelper> logger,
        IAccessControlService accessControlService,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _accessControlService = accessControlService;
        _unitOfWork = unitOfWork;
    }

    public string GetModuleNameFromPath(string path)
    {
        // Remove query strings and fragments
        var cleanPath = path.Split('?')[0].Split('#')[0].ToLower();

        // Try exact match first
        if (PageToModuleMap.TryGetValue(cleanPath, out var moduleName))
        {
            return moduleName;
        }

        // Try prefix match (e.g., /hrms/employees/form matches /hrms/employees)
        var matchingKey = PageToModuleMap.Keys
            .FirstOrDefault(key => cleanPath.StartsWith(key.ToLower(), StringComparison.OrdinalIgnoreCase));

        if (matchingKey != null)
        {
            return PageToModuleMap[matchingKey];
        }

        // Default: return path as module name for unknown pages
        return cleanPath;
    }

    public async Task<bool> HasPageAccessAsync(int userId, string pagePath, int tenantId)
    {
        try
        {
            var moduleName = GetModuleNameFromPath(pagePath);
            var accessLevel = await _accessControlService.GetAccessLevelAsync(userId, moduleName, tenantId);
            return accessLevel > 0; // Any access level > 0 means user has access
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking page access for user {UserId}, path {Path}, tenant {TenantId}", userId, pagePath, tenantId);
            return false; // Deny access on error
        }
    }

    public async Task<int> GetPageAccessLevelAsync(int userId, string pagePath, int tenantId)
    {
        try
        {
            var moduleName = GetModuleNameFromPath(pagePath);
            return await _accessControlService.GetAccessLevelAsync(userId, moduleName, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting page access level for user {UserId}, path {Path}, tenant {TenantId}", userId, pagePath, tenantId);
            return 0; // No access on error
        }
    }

    public async Task<int> GetModuleAccessLevelAsync(int userId, string moduleName, int tenantId)
    {
        try
        {
            return await _accessControlService.GetAccessLevelAsync(userId, moduleName, tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module access level for user {UserId}, module {ModuleName}, tenant {TenantId}", userId, moduleName, tenantId);
            return 0; // No access on error
        }
    }

    public async Task<Dictionary<string, int>> GetUserPagePermissionsAsync(int userId, int tenantId)
    {
        try
        {
            // Get all user permissions
            var permissions = await _accessControlService.GetUserPermissionsAsync(userId, tenantId);
            
            // Map module names to page paths
            var pagePermissions = new Dictionary<string, int>();
            foreach (var pagePath in PageToModuleMap.Keys)
            {
                var moduleName = PageToModuleMap[pagePath];
                if (permissions.TryGetValue(moduleName, out var accessLevel))
                {
                    pagePermissions[pagePath] = accessLevel;
                }
                else
                {
                    pagePermissions[pagePath] = 0; // No access
                }
            }

            return pagePermissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user page permissions for user {UserId}, tenant {TenantId}", userId, tenantId);
            return new Dictionary<string, int>();
        }
    }
}

