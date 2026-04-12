using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ICourseParticipantRepository : IRepository<CourseParticipant>
{
    Task<IEnumerable<CourseParticipantDto>> GetParticipantsByCoursePlanAsync(int coursePlanId, int tenantId);
    Task<bool> IsParticipantExistsAsync(int coursePlanId, int staffId, int tenantId);
    Task<IEnumerable<CourseParticipant>> GetByCoursePlanIdAsync(int coursePlanId, int tenantId);
    Task<CourseParticipant?> GetParticipantAsync(int coursePlanId, int staffId, int tenantId);
}
