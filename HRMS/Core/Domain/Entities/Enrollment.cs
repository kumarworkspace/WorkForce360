namespace HRMS.Core.Domain.Entities;

public class Enrollment : BaseEntity
{
    public int EnrollmentId { get; set; }
    public int StaffId { get; set; }
    public int LmsCourseId { get; set; }
    public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Enrolled";   // Enrolled | InProgress | Completed | Withdrawn
    public DateTime? CompletedDate { get; set; }
    public virtual Staff Staff { get; set; } = null!;
    public virtual LmsCourse Course { get; set; } = null!;
    public virtual ICollection<ProgressTracking> Progress { get; set; } = new List<ProgressTracking>();
}
