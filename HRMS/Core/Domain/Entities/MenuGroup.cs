namespace HRMS.Core.Domain.Entities;

public class MenuGroup : BaseEntity
{
    public int MenuGroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? PermissionModule { get; set; }
    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
