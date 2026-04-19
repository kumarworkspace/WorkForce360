namespace HRMS.Core.Domain.Entities;

public class MasterCategory : BaseEntity
{
    public int MasterCategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public virtual ICollection<MasterValue> MasterValues { get; set; } = new List<MasterValue>();
}
