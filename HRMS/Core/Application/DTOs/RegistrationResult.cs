namespace HRMS.Core.Application.DTOs;

public class RegistrationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? TenantId { get; set; }
    public int? UserId { get; set; }
}








