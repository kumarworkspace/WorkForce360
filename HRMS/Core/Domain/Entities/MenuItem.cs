namespace HRMS.Core.Domain.Entities;

public class MenuItem : BaseEntity
{
    public int MenuItemId { get; set; }
    public int MenuGroupId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? PermissionModule { get; set; }
    public int SortOrder { get; set; }
    public virtual MenuGroup MenuGroup { get; set; } = null!;
}
