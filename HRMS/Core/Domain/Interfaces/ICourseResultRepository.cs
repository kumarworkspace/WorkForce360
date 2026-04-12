using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ICourseResultRepository : IRepository<CourseResult>
{
    Task<IEnumerable<CourseResultDto>> GetResultSummaryByCoursePlanAsync(int coursePlanId, int tenantId);
    Task<CourseResult?> GetByCoursePlanStaffAsync(int coursePlanId, int staffId, int tenantId);
    Task<IEnumerable<StaffCertificateDto>> GetCertificatesByStaffIdAsync(int staffId, int tenantId);
}
