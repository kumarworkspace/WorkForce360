using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IEnrollmentService
{
    Task<IEnumerable<EnrollmentDto>> GetByStaffAsync(int staffId, int tenantId);
    Task<IEnumerable<EnrollmentDto>> GetByCourseAsync(int courseId, int tenantId);
    Task<EnrollmentDto?> GetByIdAsync(int enrollmentId, int tenantId);
    Task<(bool Success, string Message, int EnrollmentId)> EnrollAsync(EnrollRequest request);
    Task<(bool Success, string Message)> WithdrawAsync(int enrollmentId, int tenantId, string updatedBy);
    Task<(bool Success, string Message)> UpdateProgressAsync(UpdateProgressRequest request);
    Task<IEnumerable<ProgressTrackingDto>> GetProgressAsync(int enrollmentId, int tenantId);
}
