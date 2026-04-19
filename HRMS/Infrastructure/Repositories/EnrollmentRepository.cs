using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Repositories;

public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(HRMSDbContext context) : base(context) { }

    public async Task<Enrollment?> GetByStaffAndCourseAsync(int staffId, int courseId, int tenantId)
    {
        return await _dbSet.FirstOrDefaultAsync(e =>
            e.StaffId == staffId && e.LmsCourseId == courseId && e.TenantId == tenantId);
    }

    public async Task<IEnumerable<Enrollment>> GetByStaffIdAsync(int staffId, int tenantId)
    {
        return await _dbSet
            .Include(e => e.Course).ThenInclude(c => c.CourseType)
            .Include(e => e.Progress)
            .Where(e => e.StaffId == staffId && e.TenantId == tenantId && e.IsActive)
            .OrderByDescending(e => e.EnrolledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(int courseId, int tenantId)
    {
        return await _dbSet
            .Include(e => e.Staff)
            .Where(e => e.LmsCourseId == courseId && e.TenantId == tenantId && e.IsActive)
            .OrderBy(e => e.Staff.Name)
            .ToListAsync();
    }

    public async Task<Enrollment?> GetByIdAsync(int enrollmentId, int tenantId)
    {
        return await _dbSet
            .Include(e => e.Course)
            .Include(e => e.Progress)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId && e.TenantId == tenantId);
    }
}
