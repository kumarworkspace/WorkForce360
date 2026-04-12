namespace HRMS.Core.Application.DTOs;

public class PermissionDto
{
    public int PermissionId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public int TenantId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
}

public class CreatePermissionRequest
{
    public string ModuleName { get; set; } = string.Empty;
}

public class UpdatePermissionRequest
{
    public int PermissionId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}





