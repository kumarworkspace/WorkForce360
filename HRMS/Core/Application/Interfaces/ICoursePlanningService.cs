using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ICoursePlanningService
{
    Task<CoursePlanningDto?> GetByIdAsync(int id, int tenantId);
    Task<PagedResult<CoursePlanningListDto>> GetPagedAsync(
        int tenantId,
        int pageNumber,
        int pageSize,
        int? trainerId = null,
        int? courseId = null,
        bool? isActive = null,
        int? createdBy = null,
        int? staffId = null);
    Task<CoursePlanningDto> CreateAsync(CreateCoursePlanningRequest request, int tenantId, int userId, string? ipAddress);
    Task<CoursePlanningDto> UpdateAsync(UpdateCoursePlanningRequest request, int tenantId, int userId, string? ipAddress);
    Task<bool> DeleteAsync(int id, int tenantId, int userId, string? ipAddress);
    Task<ConflictValidationResult> ValidateConflictAsync(ConflictValidationRequest request);
    Task<List<string>> UploadFilesAsync(int coursePlanningId, int tenantId, List<(Stream Stream, string FileName)> files, int userId);
    Task<bool> DeleteFileAsync(int coursePlanningId, int tenantId, string filePath, int userId);
    Task<string> GenerateQRCodeAsync(int coursePlanningId, int tenantId, int userId, string? baseUrl = null);
    Task<bool> UpdateCompletionStatusAsync(int id, int tenantId, bool isCompleted, int userId);
}
