using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ICoursePlanningRepository : IRepository<CoursePlanning>
{
    Task<CoursePlanning?> GetByIdWithDetailsAsync(int id, int tenantId);
    Task<IEnumerable<CoursePlanning>> GetByTenantIdAsync(int tenantId);
    Task<IEnumerable<CoursePlanning>> GetFilteredCoursePlansAsync(
        int tenantId,
        int? trainerId = null,
        int? courseId = null,
        bool? isActive = null);
    Task<int> ValidateConflictAsync(
        int tenantId,
        int trainerId,
        DateTime startDate,
        TimeSpan startTime,
        DateTime endDate,
        TimeSpan endTime,
        int? excludeId = null);
}
