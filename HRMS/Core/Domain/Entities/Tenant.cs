namespace HRMS.Core.Domain.Entities;

public class Tenant : BaseEntity
{
    public new int TenantId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public bool IsLocked { get; set; } = false;
}


