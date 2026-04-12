using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class ExperienceDetailRepository : Repository<ExperienceDetail>, IExperienceDetailRepository
{
    public ExperienceDetailRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExperienceDetail>> GetByStaffIdAsync(int staffId, int tenantId)
    {
        return await _dbSet
            .Where(e => e.StaffId == staffId && e.TenantId == tenantId && e.IsActive)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();
    }
}
