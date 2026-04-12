using Microsoft.EntityFrameworkCore;
using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class CourseParticipantRepository : Repository<CourseParticipant>, ICourseParticipantRepository
{
    public CourseParticipantRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CourseParticipantDto>> GetParticipantsByCoursePlanAsync(int coursePlanId, int tenantId)
    {
        var result = await _dbSet
            .Where(cp => cp.CoursePlanId == coursePlanId && cp.TenantId == tenantId && cp.IsActive)
            .Include(cp => cp.Staff)
            .OrderBy(cp => cp.Staff!.Name)
            .Select(cp => new CourseParticipantDto
            {
                CourseParticipantId = cp.CourseParticipantId,
                CoursePlanId = cp.CoursePlanId,
                StaffId = cp.StaffId,
                StaffName = cp.Staff != null ? cp.Staff.Name : string.Empty,
                EmployeeCode = cp.Staff != null ? cp.Staff.EmployeeCode : null,
                Department = cp.Staff != null ? cp.Staff.Department : null,
                Position = cp.Staff != null ? cp.Staff.Position : null,
                Email = cp.Staff != null ? cp.Staff.Email : null,
                PhoneNumber = cp.Staff != null ? cp.Staff.PhoneNumber : null,
                CreatedDate = cp.CreatedDate
            })
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

    public async Task<bool> IsParticipantExistsAsync(int coursePlanId, int staffId, int tenantId)
    {
        return await _dbSet
            .AnyAsync(cp => cp.CoursePlanId == coursePlanId
                && cp.StaffId == staffId
                && cp.TenantId == tenantId
                && cp.IsActive);
    }

    public async Task<IEnumerable<CourseParticipant>> GetByCoursePlanIdAsync(int coursePlanId, int tenantId)
    {
        return await _dbSet
            .Where(cp => cp.CoursePlanId == coursePlanId && cp.TenantId == tenantId && cp.IsActive)
            .ToListAsync();
    }

    public async Task<CourseParticipant?> GetParticipantAsync(int coursePlanId, int staffId, int tenantId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(cp => cp.CoursePlanId == coursePlanId
                && cp.StaffId == staffId
                && cp.TenantId == tenantId);
    }
}
