namespace HRMS.Core.Domain.Entities;

public class SSO : BaseEntity
{
    public int SSOId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? AccessToken { get; set; }
    public DateTime? TokenExpiry { get; set; }
}
