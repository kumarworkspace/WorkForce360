using HRMS.Core.Domain.Enums;

namespace HRMS.Core.Domain.Entities;

public class AuditLog
{
    public int AuditId { get; set; }
    public int TenantId { get; set; }
    public int? UserId { get; set; }
    public ActionType ActionType { get; set; }
    public string? Description { get; set; }
    public string? IPAddress { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
