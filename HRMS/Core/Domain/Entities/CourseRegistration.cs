namespace HRMS.Core.Domain.Entities;

public class CourseRegistration
{
    public int CourseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TrainingModule { get; set; } = string.Empty;
    public int CourseTypeId { get; set; }
    public int CourseCategoryId { get; set; }
    public int TrainerId { get; set; }
    public decimal Duration { get; set; }
    public int ValidityPeriod { get; set; }
    public string? UploadFilePath { get; set; }
    public int TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }

    // Navigation properties
    public virtual MasterDropdown? CourseType { get; set; }
    public virtual MasterDropdown? CourseCategory { get; set; }
    public virtual MasterDropdown? ValidityPeriodType { get; set; }
    public virtual Staff? Trainer { get; set; }
}
