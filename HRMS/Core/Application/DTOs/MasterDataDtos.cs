namespace HRMS.Core.Application.DTOs;

public class MasterCategoryDto
{
    public int MasterCategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<MasterValueDto> Values { get; set; } = new();
}

public class MasterValueDto
{
    public int MasterValueId { get; set; }
    public int MasterCategoryId { get; set; }
    public string ValueCode { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateMasterCategoryRequest
{
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class CreateMasterValueRequest
{
    public int MasterCategoryId { get; set; }
    public string ValueCode { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class UpdateMasterValueRequest : CreateMasterValueRequest
{
    public int MasterValueId { get; set; }
    public bool IsActive { get; set; }
}
