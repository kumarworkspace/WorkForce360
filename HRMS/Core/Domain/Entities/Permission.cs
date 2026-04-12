namespace HRMS.Core.Domain.Entities;

public class Permission
{
    public int PermissionId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}





