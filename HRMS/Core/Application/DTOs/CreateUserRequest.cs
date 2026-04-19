using System.ComponentModel.DataAnnotations;

namespace HRMS.Core.Application.DTOs;

public class CreateUserRequest
{
    [Required]
    public int StaffId { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Role { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
    
    public bool AutoGeneratePassword { get; set; } = false;
}

public class CreateUserResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
}

public class UpdateUserRequest
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Role { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
}

public class ChangePasswordRequest
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

