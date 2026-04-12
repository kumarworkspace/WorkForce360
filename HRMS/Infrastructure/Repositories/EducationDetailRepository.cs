using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class EducationDetailRepository : Repository<EducationDetail>, IEducationDetailRepository
{
    public EducationDetailRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EducationDetail>> GetByStaffIdAsync(int staffId, int tenantId)
    {
        return await _dbSet
            .Where(e => e.StaffId == staffId && e.TenantId == tenantId && e.IsActive)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }
}

