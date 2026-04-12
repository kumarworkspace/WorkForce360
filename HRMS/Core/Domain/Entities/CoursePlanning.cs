namespace HRMS.Core.Domain.Entities;

public class CoursePlanning
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public DateTime StartDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public int TrainerId { get; set; }
    public string? Remarks { get; set; }
    public string? UploadFilePaths { get; set; } // Semicolon-separated file paths for multiple uploads
    public string? QRCodePath { get; set; }
    public int TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation properties
    public virtual CourseRegistration? Course { get; set; }
    public virtual Staff? Trainer { get; set; }
}
