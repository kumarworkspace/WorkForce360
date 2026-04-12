namespace HRMS.Core.Application.DTOs;

public class CreateStaffRequest
{
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
    public int? ReportingManagerId { get; set; }
    public List<EducationDetailDto> EducationDetails { get; set; } = new();
    public List<ExperienceDetailDto> ExperienceDetails { get; set; } = new();
}

public class UpdateStaffRequest : CreateStaffRequest
{
    public int StaffId { get; set; }
}

public class StaffDto
{
    public int StaffId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? GenderId { get; set; }
    public string? GenderName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? IdentityCard { get; set; }
    public string Division { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int? EmploymentStatusId { get; set; }
    public string? EmploymentStatusName { get; set; }
    public DateTime? DateJoined { get; set; }
    public DateTime? RetirementDate { get; set; }
    public string? Photo { get; set; }
    public bool IsActive { get; set; }
    public int TenantId { get; set; }
    public int? ReportingManagerId { get; set; }
    public string? ReportingManagerName { get; set; }
    public List<EducationDetailDto> EducationDetails { get; set; } = new();
    public List<ExperienceDetailDto> ExperienceDetails { get; set; } = new();
    public List<LegalDocumentDto> LegalDocuments { get; set; } = new();
}

public class EducationDetailDto
{
    public int Id { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Institution { get; set; }
    public string? Qualification { get; set; }
    public int? YearOfPassing { get; set; }
    public string? GradeOrPercentage { get; set; }
}

public class ExperienceDetailDto
{
    public int Id { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Company { get; set; }
    public string? Position { get; set; }
    public string? TotalExperience { get; set; }
}

public class LegalDocumentDto
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
