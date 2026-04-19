namespace HRMS.Core.Domain.Entities;

public class MasterValue : BaseEntity
{
    public int MasterValueId { get; set; }
    public int MasterCategoryId { get; set; }
    public string ValueCode { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public virtual MasterCategory MasterCategory { get; set; } = null!;
}
