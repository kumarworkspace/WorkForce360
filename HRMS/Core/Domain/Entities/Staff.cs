namespace HRMS.Core.Domain.Entities;

public class Staff : BaseEntity
{
    public int StaffId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? GenderId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? IdentityCard { get; set; }
    public string Division { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int? EmploymentStatusId { get; set; }
    public DateTime? DateJoined { get; set; }
    public DateTime? RetirementDate { get; set; }
    public string? Photo { get; set; }
    public int? ReportingManagerId { get; set; }
    
    // Navigation properties
    public virtual Staff? ReportingManager { get; set; }
    public virtual ICollection<Staff> DirectReports { get; set; } = new List<Staff>();
    public virtual ICollection<EducationDetail> EducationDetails { get; set; } = new List<EducationDetail>();
    public virtual ICollection<ExperienceDetail> ExperienceDetails { get; set; } = new List<ExperienceDetail>();
    public virtual ICollection<LegalDocument> LegalDocuments { get; set; } = new List<LegalDocument>();
}
