namespace HRMS.Core.Application.DTOs;

public class CompanyRegistrationRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin"; // Default role for company admin
}








