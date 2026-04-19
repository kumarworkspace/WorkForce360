using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IMasterDataService
{
    Task<IEnumerable<MasterCategoryDto>> GetCategoriesAsync(int tenantId, bool includeInactive = false);
    Task<MasterCategoryDto?> GetCategoryByIdAsync(int categoryId, int tenantId);
    Task<IEnumerable<MasterValueDto>> GetValuesByCategoryCodeAsync(string categoryCode, int tenantId);
    Task<IEnumerable<MasterValueDto>> GetValuesByCategoryIdAsync(int categoryId, bool includeInactive = false);
    Task<MasterCategoryDto> CreateCategoryAsync(CreateMasterCategoryRequest request, int tenantId, string createdBy);
    Task<MasterValueDto> CreateValueAsync(CreateMasterValueRequest request, int tenantId, string createdBy);
    Task UpdateValueAsync(UpdateMasterValueRequest request, int tenantId, string updatedBy);
    Task ToggleValueActiveAsync(int valueId, int tenantId, string updatedBy);
    Task SeedDefaultCategoriesAsync(int tenantId, string createdBy);
    Task<bool> HasCategoriesAsync(int tenantId);
}
