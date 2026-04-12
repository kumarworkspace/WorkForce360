namespace HRMS.Core.Application.DTOs;

/// <summary>
/// DTO for staff list retrieved from stored procedure usp_GetStaffList
/// </summary>
public class StaffListSpDto
{
    public int StaffId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Company { get; set; }
    public string Division { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateJoined { get; set; }
    public DateTime? RetirementDate { get; set; }
    public string? Photo { get; set; }
    public string? Address { get; set; }
    public string? IdentityCard { get; set; }
    public int? GenderId { get; set; }
    public string? GenderName { get; set; }
    public int? EmploymentStatusId { get; set; }
    public string? EmploymentStatusName { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }
    public string? ReportingManagerCode { get; set; }
    public bool IsActive { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }
}

/// <summary>
/// Request parameters for staff list stored procedure
/// </summary>
public class GetStaffListRequest
{
    public int TenantId { get; set; }
    public string? SearchTerm { get; set; }
    public string? Division { get; set; }
    public string? Department { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
