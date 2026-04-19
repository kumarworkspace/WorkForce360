namespace HRMS.Core.Domain.Entities;

public class LmsCourse : BaseEntity
{
    public int LmsCourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CourseTypeId { get; set; }
    public int? DifficultyId { get; set; }
    public string? ContentUrl { get; set; }
    public int DurationMinutes { get; set; }
    public string? Tags { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? Objectives { get; set; }
    public virtual MasterValue? CourseType { get; set; }
    public virtual MasterValue? Difficulty { get; set; }
    public virtual ICollection<LmsModule> Modules { get; set; } = new List<LmsModule>();
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<LearningPathCourse> LearningPathCourses { get; set; } = new List<LearningPathCourse>();
}
