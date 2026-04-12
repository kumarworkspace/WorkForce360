namespace HRMS.Core.Application.DTOs;

/// <summary>
/// DTO for leave request list retrieved from stored procedure usp_GetLeaveRequestList
/// </summary>
public class LeaveRequestListSpDto
{
    public int RequestId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Division { get; set; }
    public string? Position { get; set; }
    public int RequestTypeId { get; set; }
    public string? RequestTypeName { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal? TotalDays { get; set; }
    public decimal? TotalHours { get; set; }
    public string? Reason { get; set; }
    public int? LeaveStatus { get; set; }
    public string? LeaveStatusName { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }
    public string? ReportingManagerCode { get; set; }
    public bool HRApprovalRequired { get; set; }
    public int? ApprovedBy_L1 { get; set; }
    public string? ApprovedByL1Name { get; set; }
    public DateTime? ApprovedDate_L1 { get; set; }
    public int? ApprovedBy_HR { get; set; }
    public string? ApprovedByHRName { get; set; }
    public DateTime? ApprovedDate_HR { get; set; }
    public string? Attachment { get; set; }
    public bool IsActive { get; set; }
    public int TenantId { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }
}

/// <summary>
/// Request parameters for leave request list stored procedure
/// </summary>
public class GetLeaveRequestListRequest
{
    public int TenantId { get; set; }
    public int? StaffId { get; set; }
    public int? RequestTypeId { get; set; }
    public int? LeaveTypeId { get; set; }
    public int? LeaveStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
