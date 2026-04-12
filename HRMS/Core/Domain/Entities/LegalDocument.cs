namespace HRMS.Core.Domain.Entities;

public class LegalDocument : BaseEntity
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    
    // Navigation property
    public virtual Staff? Staff { get; set; }
}
