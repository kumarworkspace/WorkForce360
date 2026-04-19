using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<Enrollment?> GetByStaffAndCourseAsync(int staffId, int courseId, int tenantId);
    Task<IEnumerable<Enrollment>> GetByStaffIdAsync(int staffId, int tenantId);
    Task<IEnumerable<Enrollment>> GetByCourseIdAsync(int courseId, int tenantId);
    Task<Enrollment?> GetByIdAsync(int enrollmentId, int tenantId);
}
