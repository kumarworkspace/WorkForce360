namespace HRMS.Core.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public int TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? LoginProvider { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; } = false;
    public int FailedLoginAttempts { get; set; } = 0;
    public bool IsLocked { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int? StaffId { get; set; } // Link to Staff table
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? CreatedBy { get; set; } // Database uses int, not string
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; } // Database uses int, not string

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
