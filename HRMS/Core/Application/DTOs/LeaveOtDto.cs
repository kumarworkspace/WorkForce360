namespace HRMS.Core.Application.DTOs;

public class CreateLeaveOtRequestDto
{
    public int StaffId { get; set; }
    public int RequestTypeId { get; set; } // Leave or OT
    public int? LeaveTypeId { get; set; } // For Leave requests
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal? TotalDays { get; set; }
    public decimal? TotalHours { get; set; }
    public string? Reason { get; set; }
    public int? ReportingManagerId { get; set; }
    public bool HRApprovalRequired { get; set; } = false;
    public string? Attachment { get; set; }
}

public class UpdateLeaveOtRequestDto : CreateLeaveOtRequestDto
{
    public int RequestId { get; set; }
}

public class LeaveOtRequestDto
{
    public int RequestId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public int RequestTypeId { get; set; }
    public string RequestTypeName { get; set; } = string.Empty;
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
    public bool HRApprovalRequired { get; set; }
    public int? ApprovedBy_L1 { get; set; }
    public string? ApprovedBy_L1Name { get; set; }
    public DateTime? ApprovedDate_L1 { get; set; }
    public int? ApprovedBy_HR { get; set; }
    public string? ApprovedBy_HRName { get; set; }
    public DateTime? ApprovedDate_HR { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Attachment { get; set; }
}

public class ApproveLeaveOtRequestDto
{
    public int RequestId { get; set; }
    public bool IsApproved { get; set; }
    public string? Comments { get; set; }
}

public class LeaveTypeMasterDto
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal MaxDaysPerYear { get; set; }
    public bool IsPaid { get; set; }
    public bool IsActive { get; set; }
}

public class CreateLeaveTypeMasterDto
{
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal MaxDaysPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
}

public class UpdateLeaveTypeMasterDto : CreateLeaveTypeMasterDto
{
    public int LeaveTypeId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class HolidayMasterDto
{
    public int HolidayId { get; set; }
    public DateTime HolidayDate { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateHolidayMasterDto
{
    public DateTime HolidayDate { get; set; }
    public string HolidayName { get; set; } = string.Empty;
}

public class UpdateHolidayMasterDto : CreateHolidayMasterDto
{
    public int HolidayId { get; set; }
    public bool IsActive { get; set; } = true;
}





