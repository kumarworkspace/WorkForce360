using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ILmsCourseService
{
    Task<IEnumerable<LmsCourseDto>> GetAllAsync(int tenantId, bool includeInactive = false);
    Task<LmsCourseDto?> GetByIdAsync(int courseId, int tenantId);
    Task<(bool Success, string Message, int CourseId)> CreateAsync(CreateLmsCourseRequest request);
    Task<(bool Success, string Message)> UpdateAsync(UpdateLmsCourseRequest request);
    Task<(bool Success, string Message)> DeleteAsync(int courseId, int tenantId, string deletedBy);

    Task<IEnumerable<LmsModuleDto>> GetModulesAsync(int courseId, int tenantId);
    Task<(bool Success, string Message, int ModuleId)> CreateModuleAsync(CreateLmsModuleRequest request);
    Task<(bool Success, string Message)> DeleteModuleAsync(int moduleId, int tenantId, string deletedBy);
    Task<(bool Success, string Message)> ReorderModulesAsync(int courseId, int tenantId, List<int> orderedModuleIds);
}
