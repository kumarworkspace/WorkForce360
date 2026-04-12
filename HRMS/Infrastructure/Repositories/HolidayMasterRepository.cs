using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class HolidayMasterRepository : Repository<HolidayMaster>, IHolidayMasterRepository
{
    public HolidayMasterRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<HolidayMaster>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        var query = _dbSet.Where(h => h.TenantId == tenantId);
        
        if (!includeInactive)
        {
            query = query.Where(h => h.IsActive);
        }
        
        return await query.OrderBy(h => h.HolidayDate).ToListAsync();
    }

    public async Task<IEnumerable<HolidayMaster>> GetByDateRangeAsync(int tenantId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(h => h.TenantId == tenantId && 
                       h.IsActive && 
                       h.HolidayDate >= startDate && 
                       h.HolidayDate <= endDate)
            .OrderBy(h => h.HolidayDate)
            .ToListAsync();
    }

    public async Task<bool> HolidayDateExistsAsync(DateTime holidayDate, int tenantId, int? excludeHolidayId = null)
    {
        var query = _dbSet.Where(h => 
            h.TenantId == tenantId && 
            h.HolidayDate.Date == holidayDate.Date);
        
        if (excludeHolidayId.HasValue)
        {
            query = query.Where(h => h.HolidayId != excludeHolidayId.Value);
        }
        
        return await query.AnyAsync();
    }
}





