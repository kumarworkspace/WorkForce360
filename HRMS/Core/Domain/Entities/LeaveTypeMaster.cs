namespace HRMS.Core.Domain.Entities;

public class LeaveTypeMaster : BaseEntity
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal MaxDaysPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
}





