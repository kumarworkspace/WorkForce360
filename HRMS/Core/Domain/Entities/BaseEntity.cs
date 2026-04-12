namespace HRMS.Core.Domain.Entities;

public abstract class BaseEntity
{
    public int TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
