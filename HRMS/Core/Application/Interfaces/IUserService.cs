using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IUserService
{
    Task<CreateUserResult> CreateUserAsync(CreateUserRequest request, int tenantId, int adminUserId, string? ipAddress = null);
    Task<bool> EmailExistsAsync(string email, int tenantId, int? excludeUserId = null);
    Task<List<StaffListDto>> GetStaffWithoutUserAccountsAsync(int tenantId);
    Task<PagedResult<UserListDto>> GetPagedAsync(int tenantId, int pageNumber, int pageSize, string? searchTerm = null, string? role = null);
    Task<UserListDto?> GetByIdAsync(int userId, int tenantId);
    Task<bool> UpdateAsync(UpdateUserRequest request, int tenantId, int adminUserId, string? ipAddress = null);
    Task<bool> DeleteAsync(int userId, int tenantId, int adminUserId, string? ipAddress = null);
    Task<bool> ChangePasswordAsync(int userId, string newPassword, int tenantId, int adminUserId, string? ipAddress = null);
}

