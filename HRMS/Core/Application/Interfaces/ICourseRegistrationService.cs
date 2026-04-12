using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ICourseRegistrationService
{
    Task<CourseDto?> GetByIdAsync(int courseId, int tenantId);
    Task<PagedResult<CourseListDto>> GetPagedAsync(
        int tenantId,
        int pageNumber,
        int pageSize,
        int? courseTypeId = null,
        int? courseCategoryId = null,
        int? trainerId = null,
        bool? isActive = null,
        string? searchText = null,
        int? createdBy = null,
        int? staffId = null);
    Task<CourseDto> CreateAsync(CreateCourseRequest request, int tenantId, int userId, string? ipAddress);
    Task<CourseDto> UpdateAsync(UpdateCourseRequest request, int tenantId, int userId, string? ipAddress);
    Task<bool> DeleteAsync(int courseId, int tenantId, int userId, string? ipAddress);
    Task<string> UploadCourseFileAsync(int courseId, int tenantId, Stream fileStream, string fileName, int userId);
    Task<bool> DeleteCourseFileAsync(int courseId, int tenantId, int userId);
    Task<CourseStatisticsDto> GetCourseStatisticsAsync(int tenantId, int? createdBy = null, int? staffId = null);
    Task<List<MonthlyStatisticsDto>> GetMonthlyStatisticsAsync(int tenantId, int year, int? createdBy = null, int? staffId = null);
    Task<List<YearlyStatisticsDto>> GetYearlyStatisticsAsync(int tenantId, int? createdBy = null, int? staffId = null);
}
