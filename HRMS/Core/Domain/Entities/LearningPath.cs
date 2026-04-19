namespace HRMS.Core.Domain.Entities;

public class LearningPath : BaseEntity
{
    public int LearningPathId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? JobTitleId { get; set; }
    public virtual MasterValue? JobTitle { get; set; }
    public virtual ICollection<LearningPathCourse> Courses { get; set; } = new List<LearningPathCourse>();
}
