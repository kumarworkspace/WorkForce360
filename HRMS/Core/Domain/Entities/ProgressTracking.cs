namespace HRMS.Core.Domain.Entities;

public class ProgressTracking : BaseEntity
{
    public int ProgressTrackingId { get; set; }
    public int EnrollmentId { get; set; }
    public int LmsModuleId { get; set; }
    public decimal ProgressPct { get; set; }
    public DateTime? LastAccessedDate { get; set; }
    public bool IsCompleted { get; set; }
    public virtual Enrollment Enrollment { get; set; } = null!;
    public virtual LmsModule Module { get; set; } = null!;
}
