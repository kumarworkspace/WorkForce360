namespace HRMS.Core.Application.DTOs;

public class RolePermissionDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public int PermissionId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int AccessLevel { get; set; }
    public string AccessLevelName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public bool IsActive { get; set; }
}

public class AccessControlMatrixDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionAccessDto> Permissions { get; set; } = new();
}

public class PermissionAccessDto
{
    public int PermissionId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int AccessLevel { get; set; }
}

public class UpdateAccessControlRequest
{
    public int RoleId { get; set; }
    public List<PermissionAccessDto> Permissions { get; set; } = new();
}





