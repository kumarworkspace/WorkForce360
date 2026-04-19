namespace HRMS.Core.Domain.Entities;

public class LearningPathCourse
{
    public int LearningPathCourseId { get; set; }
    public int LearningPathId { get; set; }
    public int LmsCourseId { get; set; }
    public int TenantId { get; set; }
    public int SortOrder { get; set; }
    public virtual LearningPath LearningPath { get; set; } = null!;
    public virtual LmsCourse Course { get; set; } = null!;
}
