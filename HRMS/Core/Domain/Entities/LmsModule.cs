namespace HRMS.Core.Domain.Entities;

public class LmsModule : BaseEntity
{
    public int LmsModuleId { get; set; }
    public int LmsCourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContentUrl { get; set; }
    public string? ContentType { get; set; }   // Video | Document | Slide | Link
    public int DurationMinutes { get; set; }
    public int SortOrder { get; set; }
    public virtual LmsCourse Course { get; set; } = null!;
    public virtual ICollection<ProgressTracking> ProgressRecords { get; set; } = new List<ProgressTracking>();
}
