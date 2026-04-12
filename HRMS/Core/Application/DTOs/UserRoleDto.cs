namespace HRMS.Core.Application.DTOs;

public class UserRoleDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class UserRoleAssignmentDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<int> RoleIds { get; set; } = new();
    public List<RoleDto> AvailableRoles { get; set; } = new();
    public List<RoleDto> AssignedRoles { get; set; } = new();
}

public class AssignUserRolesRequest
{
    public int UserId { get; set; }
    public List<int> RoleIds { get; set; } = new();
}





