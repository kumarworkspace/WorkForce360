using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ICourseAttendanceDateWiseRepository : IRepository<CourseAttendanceDateWise>
{
    Task<IEnumerable<AttendanceDateWiseDto>> GetAttendanceByCoursePlanAsync(int coursePlanId, int tenantId);
    Task<CourseAttendanceDateWise?> GetByCoursePlanStaffDateAsync(int coursePlanId, int staffId, DateTime attendanceDate, int tenantId);
    Task<IEnumerable<CourseAttendanceDateWise>> GetByCoursePlanIdAsync(int coursePlanId, int tenantId);

    // CRUD Stored procedure methods
    Task<IEnumerable<AttendanceDateWiseDto>> GetAttendanceDateWiseSpAsync(int coursePlanId, int tenantId, DateTime? attendanceDate = null, int? staffId = null);
    Task<AttendanceOperationResponse> CreateAttendanceDateWiseSpAsync(CreateAttendanceDateWiseRequest request, int tenantId, int? createdBy = null);
    Task<AttendanceOperationResponse> UpdateAttendanceDateWiseSpAsync(UpdateAttendanceDateWiseByIdRequest request, int tenantId, int? updatedBy = null);
    Task<AttendanceOperationResponse> DeleteAttendanceDateWiseSpAsync(int attendanceId, int tenantId, int? updatedBy = null);
    Task<AttendanceOperationResponse> BulkMarkAttendanceDateWiseSpAsync(BulkAttendanceDateWiseRequest request, int tenantId, int? createdBy = null);
    Task<(IEnumerable<AttendanceSummaryByStaffDto> StaffSummary, IEnumerable<DailyAttendanceSummaryDto> DailySummary)> GetAttendanceSummaryByCoursePlanSpAsync(int coursePlanId, int tenantId);
}
