namespace HRMS.Core.Domain.Entities;

public class LeaveOtRequest : BaseEntity
{
    public int RequestId { get; set; }
    public int StaffId { get; set; }
    public int RequestTypeId { get; set; } // Leave or OT
    public int? LeaveTypeId { get; set; } // For Leave requests
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal? TotalDays { get; set; }
    public decimal? TotalHours { get; set; }
    public string? Reason { get; set; }
    public int? LeaveStatus { get; set; } // PENDING, APPROVED, REJECTED, CANCELLED
    public int? ReportingManagerId { get; set; }
    public bool HRApprovalRequired { get; set; } = false;
    public int? ApprovedBy_L1 { get; set; } // Level 1 (Manager) approval
    public DateTime? ApprovedDate_L1 { get; set; }
    public int? ApprovedBy_HR { get; set; } // HR approval
    public DateTime? ApprovedDate_HR { get; set; }
    public string? Attachment { get; set; } // File path for leave attachment

    // Navigation properties
    public virtual Staff? Staff { get; set; }
    public virtual LeaveTypeMaster? LeaveType { get; set; }
    public virtual Staff? ReportingManager { get; set; }
}





