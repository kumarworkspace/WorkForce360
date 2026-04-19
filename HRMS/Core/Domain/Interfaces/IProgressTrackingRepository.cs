using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IProgressTrackingRepository : IRepository<ProgressTracking>
{
    Task<IEnumerable<ProgressTracking>> GetByEnrollmentIdAsync(int enrollmentId);
    Task<ProgressTracking?> GetByEnrollmentAndModuleAsync(int enrollmentId, int moduleId);
    Task<decimal> GetOverallProgressAsync(int enrollmentId);
}
