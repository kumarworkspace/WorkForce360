namespace HRMS.Core.Domain.Entities;

public class CourseResult
{
    public int ResultId { get; set; }
    public int CoursePlanId { get; set; }
    public int StaffId { get; set; }
    public int TotalDays { get; set; } = 0;
    public int PresentDays { get; set; } = 0;
    public decimal AttendancePercentage { get; set; } = 0;
    public decimal? Marks { get; set; }
    public string? ResultStatus { get; set; } // Pass / Fail
    public string? CertificatePath { get; set; }
    public string? CertificateSerialNumber { get; set; }
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
