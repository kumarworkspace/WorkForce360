using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ICourseAttendanceService
{
    Task<MarkAttendanceResponse> MarkAttendanceAsync(MarkAttendanceRequest request, int tenantId, int userId);
    Task<IEnumerable<CourseAttendanceDto>> GetAttendanceListAsync(GetAttendanceRequest request);
    Task<IEnumerable<AttendanceSummaryDto>> GetAttendanceByCoursePlanAsync(int coursePlanId, int tenantId);
}
