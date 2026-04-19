using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class SeedingService : ISeedingService
{
    private readonly ILogger<SeedingService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    // Canonical set of permissions — single source of truth for the whole system.
    // To add a new module: add it here, update AuthorizationHelper.PageToModuleMap,
    // update NavMenu items, and update DefaultAccessMatrix below.
    public static readonly IReadOnlyList<string> CanonicalPermissions =
    [
        "Employee Management",   // /hrms/employees, /hrms/attendance
        "Leave Management",      // /hrms/leave  (submit/manage own leave)
        "Leave Approval",        // /hrms/leave/approval
        "Leave Configuration",   // /hrms/leave/types, /hrms/leave/holidays
        "My Training",           // /tms/my-courses
        "Course Management",     // /tms/courses, /tms/planning, /tms/attendance, /tms/dashboard
        "TMS Reports",           // /tms/reports/general, /tms/reports/trainer-kpi, /tms/reports/statistics
        "Role Management",       // /admin/users, /admin/roles
        "Access Control",        // /admin/access-control, /admin/user-roles
        "Audit Logs",            // /admin/audit-logs
        "Menu Management",       // /admin/menu-management
        "Master Data",           // /admin/master-data
        "LMS Courses",           // /lms/courses, /lms/my-courses
        "LMS Assessments",       // /lms/assessments
        "LMS Certificates",      // /lms/certificates
        "Learning Paths",        // /lms/learning-paths
        "Talent Pipeline",       // /talent/candidates, /talent/job-postings
    ];

    // Default access matrix — keyed by role name (case-insensitive).
    // 0=No Access, 1=View, 2=Modify, 3=Full Access.
    // Admins can override any of these via the Access Control page — this is only the initial seed.
    // Role names here must match the roles created in SeedDefaultRolesAndPermissionsAsync.
    public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> DefaultAccessMatrix =
        new Dictionary<string, IReadOnlyDictionary<string, int>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Super Admin"] = BuildFull(),
            ["IT Admin"]    = BuildFull(),
            ["HR Admin"] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "Employee Management",  3 },
                { "Leave Management",     3 },
                { "Leave Approval",       3 },
                { "Leave Configuration",  3 },
                { "My Training",          1 },
                { "Course Management",    2 },
                { "TMS Reports",          3 },
                { "Role Management",      2 },
                { "Access Control",       0 },
                { "Audit Logs",           1 },
                { "Menu Management",      2 },
                { "Master Data",          3 },
                { "LMS Courses",          2 },
                { "LMS Assessments",      1 },
                { "LMS Certificates",     1 },
                { "Learning Paths",       2 },
                { "Talent Pipeline",      2 },
            },
            ["Manager"] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "Employee Management",  2 },
                { "Leave Management",     3 },
                { "Leave Approval",       3 },
                { "Leave Configuration",  0 },
                { "My Training",          1 },
                { "Course Management",    0 },
                { "TMS Reports",          1 },
                { "Role Management",      0 },
                { "Access Control",       0 },
                { "Audit Logs",           0 },
                { "Menu Management",      0 },
                { "Master Data",          0 },
                { "LMS Courses",          1 },
                { "LMS Assessments",      1 },
                { "LMS Certificates",     1 },
                { "Learning Paths",       1 },
                { "Talent Pipeline",      2 },
            },
            ["Trainer"] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "Employee Management",  0 },
                { "Leave Management",     3 },
                { "Leave Approval",       0 },
                { "Leave Configuration",  0 },
                { "My Training",          1 },
                { "Course Management",    3 },
                { "TMS Reports",          2 },
                { "Role Management",      0 },
                { "Access Control",       0 },
                { "Audit Logs",           0 },
                { "Menu Management",      0 },
                { "Master Data",          1 },
                { "LMS Courses",          3 },
                { "LMS Assessments",      3 },
                { "LMS Certificates",     2 },
                { "Learning Paths",       2 },
                { "Talent Pipeline",      0 },
            },
            ["Staff"] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "Employee Management",  0 },
                { "Leave Management",     3 },
                { "Leave Approval",       0 },
                { "Leave Configuration",  0 },
                { "My Training",          1 },
                { "Course Management",    0 },
                { "TMS Reports",          0 },
                { "Role Management",      0 },
                { "Access Control",       0 },
                { "Audit Logs",           0 },
                { "Menu Management",      0 },
                { "Master Data",          0 },
                { "LMS Courses",          1 },
                { "LMS Assessments",      1 },
                { "LMS Certificates",     1 },
                { "Learning Paths",       1 },
                { "Talent Pipeline",      0 },
            },
        };

    private static IReadOnlyDictionary<string, int> BuildFull() =>
        CanonicalPermissions.ToDictionary(p => p, _ => 3, StringComparer.OrdinalIgnoreCase);

    public SeedingService(ILogger<SeedingService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedDefaultRolesAndPermissionsAsync(int tenantId, int createdByUserId)
    {
        try
        {
            if (await HasRolesAndPermissionsAsync(tenantId))
            {
                _logger.LogInformation("Roles and permissions already exist for tenant: {TenantId}", tenantId);
                return;
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // 1. Create permissions from canonical list
                var permissionDict = new Dictionary<string, Permission>(StringComparer.OrdinalIgnoreCase);
                foreach (var moduleName in CanonicalPermissions)
                {
                    var p = new Permission
                    {
                        ModuleName  = moduleName,
                        TenantId    = tenantId,
                        IsActive    = true,
                        CreatedBy   = createdByUserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.Permission.AddAsync(p);
                    permissionDict[moduleName] = p;
                }
                await _unitOfWork.SaveChangesAsync();

                // 2. Create default roles
                var defaultRoles = new[]
                {
                    ("Super Admin", "Full system access with all permissions"),
                    ("HR Admin",    "HR Administrator with full HR access"),
                    ("Manager",     "Manager with view and modify access"),
                    ("Trainer",     "Training management access"),
                    ("Staff",       "Staff with leave and training access"),
                    ("IT Admin",    "IT Administrator with full access"),
                };

                var roleDict = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
                foreach (var (roleName, desc) in defaultRoles)
                {
                    var r = new Role
                    {
                        RoleName    = roleName,
                        Description = desc,
                        TenantId    = tenantId,
                        IsActive    = true,
                        CreatedBy   = createdByUserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.Role.AddAsync(r);
                    roleDict[roleName] = r;
                }
                await _unitOfWork.SaveChangesAsync();

                // 3. Seed role-permission entries from DefaultAccessMatrix
                foreach (var (roleName, permMap) in DefaultAccessMatrix)
                {
                    if (!roleDict.TryGetValue(roleName, out var role)) continue;

                    foreach (var (moduleName, level) in permMap)
                    {
                        if (!permissionDict.TryGetValue(moduleName, out var perm)) continue;
                        if (level <= 0) continue;

                        await _unitOfWork.RolePermission.AddAsync(new RolePermission
                        {
                            RoleId       = role.RoleId,
                            PermissionId = perm.PermissionId,
                            AccessLevel  = level,
                            TenantId     = tenantId,
                            IsActive     = true,
                            CreatedBy    = createdByUserId,
                            CreatedDate  = DateTime.UtcNow
                        });
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Seeded default roles and permissions for tenant {TenantId}", tenantId);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default roles and permissions for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> HasRolesAndPermissionsAsync(int tenantId)
    {
        try
        {
            var roles       = await _unitOfWork.Role.GetByTenantIdAsync(tenantId);
            var permissions = await _unitOfWork.Permission.GetByTenantIdAsync(tenantId);
            return roles.Any() && permissions.Any();
        }
        catch { return false; }
    }

    /// <summary>
    /// Seeds RolePermission entries for roles that have none yet.
    /// Safe to call repeatedly — skips roles that already have entries.
    /// </summary>
    public async Task EnsureRolePermissionsSeededAsync(int tenantId, int createdByUserId)
    {
        try
        {
            var roles       = (await _unitOfWork.Role.GetByTenantIdAsync(tenantId, includeInactive: false)).ToList();
            var permissions = (await _unitOfWork.Permission.GetByTenantIdAsync(tenantId)).ToList();
            if (!roles.Any() || !permissions.Any()) return;

            bool anyAdded = false;
            foreach (var role in roles)
            {
                var existing = await _unitOfWork.RolePermission.GetByRoleIdAsync(role.RoleId, tenantId);
                if (existing.Any()) continue;

                if (!DefaultAccessMatrix.TryGetValue(role.RoleName, out var permMap)) continue;

                foreach (var perm in permissions)
                {
                    if (!permMap.TryGetValue(perm.ModuleName, out var level) || level <= 0) continue;

                    await _unitOfWork.RolePermission.AddAsync(new RolePermission
                    {
                        RoleId       = role.RoleId,
                        PermissionId = perm.PermissionId,
                        AccessLevel  = level,
                        TenantId     = tenantId,
                        IsActive     = true,
                        CreatedBy    = createdByUserId,
                        CreatedDate  = DateTime.UtcNow
                    });
                    anyAdded = true;
                }

                _logger.LogInformation(
                    "EnsureRolePermissions: seeded permissions for role '{Role}' tenant {TenantId}",
                    role.RoleName, tenantId);
            }

            if (anyAdded) await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring role permissions for tenant {TenantId}", tenantId);
            throw;
        }
    }

    /// <summary>
    /// Adds any canonical permissions that are missing from the tenant's Permission table,
    /// then seeds default RolePermission entries for those new permissions.
    /// Call this at startup after adding new entries to CanonicalPermissions.
    /// </summary>
    public async Task EnsureMissingPermissionsAsync(int tenantId, int createdByUserId)
    {
        try
        {
            var existing = (await _unitOfWork.Permission.GetByTenantIdAsync(tenantId)).ToList();
            var existingNames = existing.Select(p => p.ModuleName).ToHashSet(StringComparer.OrdinalIgnoreCase);

            var missing = CanonicalPermissions.Where(name => !existingNames.Contains(name)).ToList();
            if (!missing.Any()) return;

            var roles = (await _unitOfWork.Role.GetByTenantIdAsync(tenantId, includeInactive: false)).ToList();

            foreach (var moduleName in missing)
            {
                var perm = new Permission
                {
                    ModuleName  = moduleName,
                    TenantId    = tenantId,
                    IsActive    = true,
                    CreatedBy   = createdByUserId,
                    CreatedDate = DateTime.UtcNow
                };
                await _unitOfWork.Permission.AddAsync(perm);
                await _unitOfWork.SaveChangesAsync(); // get PermissionId

                // Seed role-permission for each role that has a default for this module
                foreach (var role in roles)
                {
                    if (!DefaultAccessMatrix.TryGetValue(role.RoleName, out var permMap)) continue;
                    if (!permMap.TryGetValue(moduleName, out var level) || level <= 0) continue;

                    await _unitOfWork.RolePermission.AddAsync(new RolePermission
                    {
                        RoleId       = role.RoleId,
                        PermissionId = perm.PermissionId,
                        AccessLevel  = level,
                        TenantId     = tenantId,
                        IsActive     = true,
                        CreatedBy    = createdByUserId,
                        CreatedDate  = DateTime.UtcNow
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation(
                    "EnsureMissingPermissions: added '{Module}' for tenant {TenantId}", moduleName, tenantId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring missing permissions for tenant {TenantId}", tenantId);
            throw;
        }
    }
}
