using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IMenuService
{
    Task<IEnumerable<MenuGroupDto>> GetMenuAsync(int tenantId, bool includeInactive = false);
    Task<MenuGroupDto?> GetGroupByIdAsync(int menuGroupId, int tenantId);
    Task<MenuGroupDto> CreateGroupAsync(CreateMenuGroupRequest request, int tenantId, string createdBy);
    Task UpdateGroupAsync(UpdateMenuGroupRequest request, int tenantId, string updatedBy);
    Task DeleteGroupAsync(int menuGroupId, int tenantId, string updatedBy);
    Task<MenuItemDto> CreateItemAsync(CreateMenuItemRequest request, int tenantId, string createdBy);
    Task UpdateItemAsync(UpdateMenuItemRequest request, int tenantId, string updatedBy);
    Task DeleteItemAsync(int menuItemId, int tenantId, string updatedBy);
    Task ReorderGroupsAsync(List<int> orderedGroupIds, int tenantId, string updatedBy);
    Task ReorderItemsAsync(int groupId, List<int> orderedItemIds, int tenantId, string updatedBy);
    Task SeedDefaultMenuAsync(int tenantId, string createdBy);
    Task<bool> HasMenuAsync(int tenantId);
}
