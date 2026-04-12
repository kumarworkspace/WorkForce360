namespace HRMS.Core.Application.DTOs;

public class LeaveBalanceDto
{
    public int LeaveBalanceId { get; set; }
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public int Year { get; set; }
    public bool IsActive { get; set; }
}

public class CreateLeaveBalanceDto
{
    public int StaffId { get; set; }
    public int LeaveTypeId { get; set; }
    public decimal TotalDays { get; set; }
    public int Year { get; set; }
}

public class UpdateLeaveBalanceDto
{
    public int LeaveBalanceId { get; set; }
    public decimal UsedDays { get; set; }
}





