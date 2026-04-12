namespace HRMS.Core.Domain.Entities;

public class CourseAttendanceDateWise
{
    public int AttendanceId { get; set; }
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public bool IsPresent { get; set; } = true;
    public string? Remarks { get; set; }
    public int TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public virtual CoursePlanning? CoursePlan { get; set; }
    public virtual Staff? Staff { get; set; }
}
