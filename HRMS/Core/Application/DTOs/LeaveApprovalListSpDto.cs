namespace HRMS.Core.Application.DTOs;

/// <summary>
/// DTO for leave approval list retrieved from stored procedure usp_GetLeaveApprovalList
/// </summary>
public class LeaveApprovalListSpDto
{
    public int RequestId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Division { get; set; }
    public string? Position { get; set; }
    public string? StaffPhoto { get; set; }
    public int RequestTypeId { get; set; }
    public string? RequestTypeName { get; set; }
    public string? RequestTypeCode { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal? TotalDays { get; set; }
    public decimal? TotalHours { get; set; }
    public string? Reason { get; set; }
    public int? LeaveStatus { get; set; }
    public string? LeaveStatusName { get; set; }
    public string? LeaveStatusCode { get; set; }
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
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? ApprovalStage { get; set; }
    public int DaysUntilStart { get; set; }
}

/// <summary>
/// Request parameters for leave approval list stored procedure
/// </summary>
public class GetLeaveApprovalListRequest
{
    public int TenantId { get; set; }
    public int? ReportingManagerId { get; set; }
    public bool ForHRApproval { get; set; }
    public int? PendingStatusId { get; set; }
    public int? ApprovedStatusId { get; set; }
    public int? LeaveStatus { get; set; }
    public int? RequestTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
    public bool ShowAllRequests { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// DTO for pending approvals by manager
/// </summary>
public class PendingApprovalByManagerDto
{
    public int RequestId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Division { get; set; }
    public string? Position { get; set; }
    public string? StaffPhoto { get; set; }
    public int RequestTypeId { get; set; }
    public string? RequestTypeName { get; set; }
    public string? RequestTypeCode { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal? TotalDays { get; set; }
    public decimal? TotalHours { get; set; }
    public string? Reason { get; set; }
    public int? LeaveStatus { get; set; }
    public string? LeaveStatusName { get; set; }
    public bool HRApprovalRequired { get; set; }
    public string? Attachment { get; set; }
    public DateTime CreatedDate { get; set; }
    public int DaysUntilStart { get; set; }
}

/// <summary>
/// DTO for pending approvals for HR (standalone class, not inheriting to avoid EF Core keyless entity issues)
/// </summary>
public class PendingApprovalForHRDto
{
    public int RequestId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public string? Division { get; set; }
    public string? Position { get; set; }
    public string? StaffPhoto { get; set; }
    public int RequestTypeId { get; set; }
    public string? RequestTypeName { get; set; }
    public string? RequestTypeCode { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? LeaveTypeName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal? TotalDays { get; set; }
    public decimal? TotalHours { get; set; }
    public string? Reason { get; set; }
    public int? LeaveStatus { get; set; }
    public string? LeaveStatusName { get; set; }
    public bool HRApprovalRequired { get; set; }
    public string? Attachment { get; set; }
    public DateTime CreatedDate { get; set; }
    public int DaysUntilStart { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }
    public int? ApprovedBy_L1 { get; set; }
    public string? ApprovedByL1Name { get; set; }
    public DateTime? ApprovedDate_L1 { get; set; }
    public string? ApprovalStage { get; set; }
}
