namespace HRMS.Core.Domain.Entities;

public class ExperienceDetail : BaseEntity
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Company { get; set; }
    public string? Position { get; set; }
    public string? TotalExperience { get; set; }
    
    // Navigation property
    public virtual Staff? Staff { get; set; }
}
