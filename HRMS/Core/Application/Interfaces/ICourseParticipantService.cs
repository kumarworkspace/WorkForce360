using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ICourseParticipantService
{
    Task<CourseDetailWithParticipantsDto?> GetCourseWithParticipantsAsync(int coursePlanId, int tenantId);
    Task<bool> AddParticipantsAsync(AddCourseParticipantRequest request, int tenantId, int userId);
    Task<bool> RemoveParticipantAsync(int coursePlanId, int staffId, int tenantId, int userId);
    Task<IEnumerable<AttendanceGridDto>> GetAttendanceGridAsync(int coursePlanId, int tenantId);
    Task<bool> UpdateAttendanceAsync(UpdateAttendanceRequest request, int tenantId, int userId);
    Task<IEnumerable<CourseResultDto>> GetResultSummaryAsync(int coursePlanId, int tenantId);
    Task<bool> UpdateResultAsync(UpdateResultRequest request, int tenantId, int userId);
    Task<bool> UpdateMarksAsync(int coursePlanId, int staffId, decimal? marks, int tenantId, int userId);
    Task<string?> GenerateCertificateAsync(int coursePlanId, int staffId, int tenantId, int userId);

    // Date-wise Attendance CRUD Stored Procedure Methods
    Task<IEnumerable<AttendanceDateWiseDto>> GetAttendanceDateWiseSpAsync(int coursePlanId, int tenantId, DateTime? attendanceDate = null, int? staffId = null);
    Task<AttendanceOperationResponse> CreateAttendanceDateWiseSpAsync(CreateAttendanceDateWiseRequest request, int tenantId, int? createdBy = null);
    Task<AttendanceOperationResponse> UpdateAttendanceDateWiseSpAsync(UpdateAttendanceDateWiseByIdRequest request, int tenantId, int? updatedBy = null);
    Task<AttendanceOperationResponse> DeleteAttendanceDateWiseSpAsync(int attendanceId, int tenantId, int? updatedBy = null);
    Task<AttendanceOperationResponse> BulkMarkAttendanceDateWiseSpAsync(BulkAttendanceDateWiseRequest request, int tenantId, int? createdBy = null);
    Task<(IEnumerable<AttendanceSummaryByStaffDto> StaffSummary, IEnumerable<DailyAttendanceSummaryDto> DailySummary)> GetAttendanceSummaryByCoursePlanSpAsync(int coursePlanId, int tenantId);

    // My Courses (for staff participants)
    Task<IEnumerable<MyCourseSummaryDto>> GetMyCoursesAsync(int staffId, int tenantId);

    // Staff Certificate Methods
    Task<IEnumerable<StaffCertificateDto>> GetCertificatesByStaffIdAsync(int staffId, int tenantId);
}
