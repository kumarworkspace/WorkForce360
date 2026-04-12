namespace HRMS.Core.Domain.Entities;

public class UserRole
{
    public int Id { get; set; }
    public int UserId { get; set; } // Now matches Users table UserId (int)
    public int RoleId { get; set; }
    public int TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}

