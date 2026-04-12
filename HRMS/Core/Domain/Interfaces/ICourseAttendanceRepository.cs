using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ICourseAttendanceRepository : IRepository<CourseAttendance>
{
    Task<MarkAttendanceResponse> MarkAttendanceAsync(
        int userId,
        int coursePlanId,
        int staffId,
        int tenantId,
        TimeSpan? checkInTime = null,
        int? createdBy = null);

    Task<IEnumerable<CourseAttendanceDto>> GetCourseAttendanceAsync(
        int tenantId,
        int? coursePlanId = null,
        int? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceByCoursePlanAsync(
        int coursePlanId,
        int tenantId);
}
