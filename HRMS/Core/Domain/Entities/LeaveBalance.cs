namespace HRMS.Core.Domain.Entities;

public class LeaveBalance
{
    public int LeaveBalanceId { get; set; }
    public int TenantId { get; set; }
    public int StaffId { get; set; }
    public int LeaveTypeId { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public int Year { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public virtual Staff Staff { get; set; } = null!;
    public virtual LeaveTypeMaster LeaveType { get; set; } = null!;
}





