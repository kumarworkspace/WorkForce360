namespace HRMS.Core.Domain.Entities;

public class EducationDetail : BaseEntity
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Institution { get; set; }
    public string? Qualification { get; set; }
    public int? YearOfPassing { get; set; }
    public string? GradeOrPercentage { get; set; }
    
    // Navigation property
    public virtual Staff? Staff { get; set; }
}
