namespace HRMS.Core.Application.DTOs;

public class MenuGroupDto
{
    public int MenuGroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? PermissionModule { get; set; }
    public bool IsActive { get; set; }
    public List<MenuItemDto> MenuItems { get; set; } = new();
}

public class MenuItemDto
{
    public int MenuItemId { get; set; }
    public int MenuGroupId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? PermissionModule { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateMenuGroupRequest
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? PermissionModule { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateMenuGroupRequest : CreateMenuGroupRequest
{
    public int MenuGroupId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateMenuItemRequest
{
    public int MenuGroupId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? PermissionModule { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateMenuItemRequest : CreateMenuItemRequest
{
    public int MenuItemId { get; set; }
    public bool IsActive { get; set; }
}
