using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class MenuService : IMenuService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<MenuService> _logger;

    // Default menu structure — single source of truth for seeding new tenants
    private static readonly List<(string Title, string Icon, string? PermModule, List<(string Label, string Href, string Icon, string? PermModule)> Items)> DefaultMenus =
    [
        ("WorkForce360", "Icons.Material.Filled.People", null,
        [
            ("Employee Management", "/hrms/employees",      "Icons.Material.Filled.Person",       "Employee Management"),
            ("Staff Attendance",    "/hrms/attendance",     "Icons.Material.Filled.HowToReg",     "Employee Management"),
            ("My Leave",            "/hrms/leave",          "Icons.Material.Filled.CalendarToday","Leave Management"),
            ("Leave Approvals",     "/hrms/leave/approval", "Icons.Material.Filled.CheckCircle",  "Leave Approval"),
            ("Leave Configuration", "/hrms/leave/types",    "Icons.Material.Filled.List",         "Leave Configuration"),
            ("Holidays",            "/hrms/leave/holidays", "Icons.Material.Filled.BeachAccess",  "Leave Configuration"),
        ]),
        ("My Training", "Icons.Material.Filled.School", null,
        [
            ("My Courses", "/tms/my-courses", "Icons.Material.Filled.School", "My Training"),
        ]),
        ("TMS", "Icons.Material.Filled.CastForEducation", null,
        [
            ("TMS Dashboard",     "/tms/dashboard",  "Icons.Material.Filled.CastForEducation", "Course Management"),
            ("Course Management", "/tms/courses",    "Icons.Material.Filled.Book",             "Course Management"),
            ("Course Planning",   "/tms/planning",   "Icons.Material.Filled.EventNote",        "Course Management"),
        ]),
        ("Reports", "Icons.Material.Filled.Assessment", null,
        [
            ("General Report",    "/tms/reports/general",     "Icons.Material.Filled.TableChart", "TMS Reports"),
            ("Trainer KPI",       "/tms/reports/trainer-kpi", "Icons.Material.Filled.Person",     "TMS Reports"),
            ("Statistics Report", "/tms/reports/statistics",  "Icons.Material.Filled.BarChart",   "TMS Reports"),
        ]),
        ("LMS", "Icons.Material.Filled.MenuBook", null,
        [
            ("LMS Courses",      "/lms/courses",         "Icons.Material.Filled.LibraryBooks", "LMS Courses"),
            ("My LMS Courses",   "/lms/my-courses",      "Icons.Material.Filled.School",       "LMS Courses"),
            ("Learning Paths",   "/lms/learning-paths",  "Icons.Material.Filled.Route",        "Learning Paths"),
            ("Assessments",      "/lms/assessments",     "Icons.Material.Filled.Quiz",         "LMS Assessments"),
            ("Certificates",     "/lms/certificates",    "Icons.Material.Filled.CardMembership","LMS Certificates"),
            ("AI Tutor",         "/lms/ai-tutor",        "Icons.Material.Filled.SmartToy",     "LMS Courses"),
        ]),
        ("Talent", "Icons.Material.Filled.Work", null,
        [
            ("Candidates",   "/talent/candidates",   "Icons.Material.Filled.People",    "Talent Pipeline"),
            ("Job Postings", "/talent/job-postings", "Icons.Material.Filled.WorkOutline","Talent Pipeline"),
            ("Applications", "/talent/applications", "Icons.Material.Filled.Assignment", "Talent Pipeline"),
        ]),
        ("Admin", "Icons.Material.Filled.AdminPanelSettings", null,
        [
            ("User Management",  "/admin/users",            "Icons.Material.Filled.People",          "Role Management"),
            ("Role Management",  "/admin/roles",            "Icons.Material.Filled.Assignment",       "Role Management"),
            ("Access Control",   "/admin/access-control",   "Icons.Material.Filled.Security",         "Access Control"),
            ("Menu Management",  "/admin/menu-management",  "Icons.Material.Filled.Menu",             "Menu Management"),
            ("Master Data",      "/admin/master-data",      "Icons.Material.Filled.Storage",          "Master Data"),
            ("Audit Logs",       "/admin/audit-logs",       "Icons.Material.Filled.ListAlt",          "Audit Logs"),
        ]),
    ];

    public MenuService(IUnitOfWork uow, ILogger<MenuService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<IEnumerable<MenuGroupDto>> GetMenuAsync(int tenantId, bool includeInactive = false)
    {
        var groups = await _uow.MenuGroup.GetWithItemsAsync(tenantId);
        return groups.Select(ToDto);
    }

    public async Task<MenuGroupDto?> GetGroupByIdAsync(int menuGroupId, int tenantId)
    {
        var g = await _uow.MenuGroup.GetByIdAsync(menuGroupId, tenantId);
        return g is null ? null : ToDto(g);
    }

    public async Task<MenuGroupDto> CreateGroupAsync(CreateMenuGroupRequest req, int tenantId, string createdBy)
    {
        var entity = new MenuGroup
        {
            TenantId        = tenantId,
            Title           = req.Title,
            Icon            = req.Icon,
            SortOrder       = req.SortOrder,
            PermissionModule= req.PermissionModule,
            IsActive        = true,
            CreatedBy       = createdBy,
            CreatedDate     = DateTime.UtcNow
        };
        await _uow.MenuGroup.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task UpdateGroupAsync(UpdateMenuGroupRequest req, int tenantId, string updatedBy)
    {
        var entity = await _uow.MenuGroup.GetByIdAsync(req.MenuGroupId, tenantId)
                     ?? throw new KeyNotFoundException($"MenuGroup {req.MenuGroupId} not found");
        entity.Title           = req.Title;
        entity.Icon            = req.Icon;
        entity.SortOrder       = req.SortOrder;
        entity.PermissionModule= req.PermissionModule;
        entity.IsActive        = req.IsActive;
        entity.UpdatedBy       = updatedBy;
        entity.UpdatedDate     = DateTime.UtcNow;
        await _uow.MenuGroup.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteGroupAsync(int menuGroupId, int tenantId, string updatedBy)
    {
        var entity = await _uow.MenuGroup.GetByIdAsync(menuGroupId, tenantId)
                     ?? throw new KeyNotFoundException($"MenuGroup {menuGroupId} not found");
        entity.IsActive    = false;
        entity.UpdatedBy   = updatedBy;
        entity.UpdatedDate = DateTime.UtcNow;
        await _uow.MenuGroup.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task<MenuItemDto> CreateItemAsync(CreateMenuItemRequest req, int tenantId, string createdBy)
    {
        var entity = new MenuItem
        {
            TenantId        = tenantId,
            MenuGroupId     = req.MenuGroupId,
            Label           = req.Label,
            Href            = req.Href,
            Icon            = req.Icon,
            PermissionModule= req.PermissionModule,
            SortOrder       = req.SortOrder,
            IsActive        = true,
            CreatedBy       = createdBy,
            CreatedDate     = DateTime.UtcNow
        };
        await _uow.MenuItem.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return ToItemDto(entity);
    }

    public async Task UpdateItemAsync(UpdateMenuItemRequest req, int tenantId, string updatedBy)
    {
        var entity = await _uow.MenuItem.GetByIdAsync(req.MenuItemId, tenantId)
                     ?? throw new KeyNotFoundException($"MenuItem {req.MenuItemId} not found");
        entity.Label           = req.Label;
        entity.Href            = req.Href;
        entity.Icon            = req.Icon;
        entity.PermissionModule= req.PermissionModule;
        entity.SortOrder       = req.SortOrder;
        entity.IsActive        = req.IsActive;
        entity.UpdatedBy       = updatedBy;
        entity.UpdatedDate     = DateTime.UtcNow;
        await _uow.MenuItem.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(int menuItemId, int tenantId, string updatedBy)
    {
        var entity = await _uow.MenuItem.GetByIdAsync(menuItemId, tenantId)
                     ?? throw new KeyNotFoundException($"MenuItem {menuItemId} not found");
        entity.IsActive    = false;
        entity.UpdatedBy   = updatedBy;
        entity.UpdatedDate = DateTime.UtcNow;
        await _uow.MenuItem.UpdateAsync(entity);
        await _uow.SaveChangesAsync();
    }

    public async Task ReorderGroupsAsync(List<int> orderedGroupIds, int tenantId, string updatedBy)
    {
        var groups = (await _uow.MenuGroup.GetByTenantIdAsync(tenantId, true)).ToList();
        for (int i = 0; i < orderedGroupIds.Count; i++)
        {
            var g = groups.FirstOrDefault(x => x.MenuGroupId == orderedGroupIds[i]);
            if (g is null) continue;
            g.SortOrder  = i;
            g.UpdatedBy  = updatedBy;
            g.UpdatedDate= DateTime.UtcNow;
            await _uow.MenuGroup.UpdateAsync(g);
        }
        await _uow.SaveChangesAsync();
    }

    public async Task ReorderItemsAsync(int groupId, List<int> orderedItemIds, int tenantId, string updatedBy)
    {
        var items = (await _uow.MenuItem.GetByGroupIdAsync(groupId, tenantId)).ToList();
        for (int i = 0; i < orderedItemIds.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.MenuItemId == orderedItemIds[i]);
            if (item is null) continue;
            item.SortOrder  = i;
            item.UpdatedBy  = updatedBy;
            item.UpdatedDate= DateTime.UtcNow;
            await _uow.MenuItem.UpdateAsync(item);
        }
        await _uow.SaveChangesAsync();
    }

    public async Task<bool> HasMenuAsync(int tenantId)
    {
        var groups = await _uow.MenuGroup.GetByTenantIdAsync(tenantId);
        return groups.Any();
    }

    public async Task SeedDefaultMenuAsync(int tenantId, string createdBy)
    {
        if (await HasMenuAsync(tenantId)) return;

        for (int gi = 0; gi < DefaultMenus.Count; gi++)
        {
            var (gTitle, gIcon, gPermModule, items) = DefaultMenus[gi];
            var group = new MenuGroup
            {
                TenantId        = tenantId,
                Title           = gTitle,
                Icon            = gIcon,
                SortOrder       = gi,
                PermissionModule= gPermModule,
                IsActive        = true,
                CreatedBy       = createdBy,
                CreatedDate     = DateTime.UtcNow
            };
            await _uow.MenuGroup.AddAsync(group);
            await _uow.SaveChangesAsync();

            for (int ii = 0; ii < items.Count; ii++)
            {
                var (iLabel, iHref, iIcon, iPermModule) = items[ii];
                await _uow.MenuItem.AddAsync(new MenuItem
                {
                    TenantId        = tenantId,
                    MenuGroupId     = group.MenuGroupId,
                    Label           = iLabel,
                    Href            = iHref,
                    Icon            = iIcon,
                    PermissionModule= iPermModule,
                    SortOrder       = ii,
                    IsActive        = true,
                    CreatedBy       = createdBy,
                    CreatedDate     = DateTime.UtcNow
                });
            }
            await _uow.SaveChangesAsync();
        }
        _logger.LogInformation("Seeded default menu for tenant {TenantId}", tenantId);
    }

    private static MenuGroupDto ToDto(MenuGroup g) => new()
    {
        MenuGroupId     = g.MenuGroupId,
        Title           = g.Title,
        Icon            = g.Icon,
        SortOrder       = g.SortOrder,
        PermissionModule= g.PermissionModule,
        IsActive        = g.IsActive,
        MenuItems       = g.MenuItems.OrderBy(i => i.SortOrder).Select(ToItemDto).ToList()
    };

    private static MenuItemDto ToItemDto(MenuItem i) => new()
    {
        MenuItemId      = i.MenuItemId,
        MenuGroupId     = i.MenuGroupId,
        Label           = i.Label,
        Href            = i.Href,
        Icon            = i.Icon,
        PermissionModule= i.PermissionModule,
        SortOrder       = i.SortOrder,
        IsActive        = i.IsActive
    };
}
