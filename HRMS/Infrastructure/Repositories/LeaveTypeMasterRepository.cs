using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class LeaveTypeMasterRepository : Repository<LeaveTypeMaster>, ILeaveTypeMasterRepository
{
    public LeaveTypeMasterRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveTypeMaster>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        var query = _dbSet.Where(l => l.TenantId == tenantId);
        
        if (!includeInactive)
        {
            query = query.Where(l => l.IsActive);
        }
        
        return await query.OrderBy(l => l.LeaveTypeName).ToListAsync();
    }

    public async Task<bool> LeaveTypeNameExistsAsync(string leaveTypeName, int tenantId, int? excludeLeaveTypeId = null)
    {
        var query = _dbSet.Where(l => 
            l.TenantId == tenantId && 
            l.LeaveTypeName.ToLower() == leaveTypeName.ToLower());
        
        if (excludeLeaveTypeId.HasValue)
        {
            query = query.Where(l => l.LeaveTypeId != excludeLeaveTypeId.Value);
        }
        
        return await query.AnyAsync();
    }
}





