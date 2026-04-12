namespace HRMS.Core.Domain.ValueObjects;

public class PeriodicRequestSettings
{
    public bool IsEnabled { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? NextDueDate { get; set; }
}
