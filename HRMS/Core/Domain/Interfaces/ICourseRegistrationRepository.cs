using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ICourseRegistrationRepository : IRepository<CourseRegistration>
{
    Task<CourseRegistration?> GetByIdWithDetailsAsync(int courseId, int tenantId);
    Task<IEnumerable<CourseRegistration>> GetByTenantIdAsync(int tenantId);
    Task<bool> CodeExistsAsync(string code, int tenantId, int? excludeCourseId = null);
    Task<IEnumerable<CourseRegistration>> GetFilteredCoursesAsync(
        int tenantId,
        int? courseTypeId = null,
        int? courseCategoryId = null,
        int? trainerId = null,
        bool? isActive = null,
        string? searchText = null);
}
