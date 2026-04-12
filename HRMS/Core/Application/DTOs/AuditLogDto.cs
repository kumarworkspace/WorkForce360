namespace HRMS.Core.Application.DTOs;

public class AuditLogDto
{
    public int AuditId { get; set; }
    public int TenantId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IPAddress { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
}





