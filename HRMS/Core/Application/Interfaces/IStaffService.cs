using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IStaffService
{
    Task<StaffDto?> GetByIdAsync(int staffId, int tenantId);
    Task<PagedResult<StaffListDto>> GetPagedAsync(int tenantId, int pageNumber, int pageSize, string? searchTerm, string? division, string? department);
    Task<StaffDto> CreateAsync(CreateStaffRequest request, int tenantId, int userId, string? ipAddress);
    Task<StaffDto> UpdateAsync(UpdateStaffRequest request, int tenantId, int userId, string? ipAddress);
    Task<bool> DeleteAsync(int staffId, int tenantId, int userId, string? ipAddress);
    Task<string> UploadPhotoAsync(int staffId, int tenantId, Stream fileStream, string fileName, int userId);
    Task<string> UploadDocumentAsync(int staffId, int tenantId, string documentType, Stream fileStream, string fileName, int userId);
    Task<bool> DeleteDocumentAsync(int documentId, int tenantId, int userId);
    Task LogViewAsync(int staffId, int tenantId, int userId, string? ipAddress);
    Task<StaffDto?> GetLastEmployeeCodeAsync(int tenantId);

    Task<StaffDto?> GetByUserIdAsync(int userId);

    // Stored procedure methods
    Task<PagedResult<StaffListSpDto>> GetStaffListSpAsync(GetStaffListRequest request);
}

