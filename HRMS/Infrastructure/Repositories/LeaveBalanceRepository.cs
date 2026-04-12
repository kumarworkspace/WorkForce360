using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class LeaveBalanceRepository : Repository<LeaveBalance>, ILeaveBalanceRepository
{
    public LeaveBalanceRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<LeaveBalance?> GetByStaffAndLeaveTypeAsync(int staffId, int leaveTypeId, int year, int tenantId)
    {
        return await _dbSet
            .Where(lb => lb.StaffId == staffId && 
                        lb.LeaveTypeId == leaveTypeId && 
                        lb.Year == year && 
                        lb.TenantId == tenantId &&
                        lb.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<LeaveBalance>> GetByStaffIdAsync(int staffId, int tenantId, int? year = null)
    {
        IQueryable<LeaveBalance> query = _dbSet
            .Where(lb => lb.StaffId == staffId && lb.TenantId == tenantId && lb.IsActive)
            .Include(lb => lb.LeaveType);

        if (year.HasValue)
        {
            query = query.Where(lb => lb.Year == year.Value);
        }

        return await query.OrderBy(lb => lb.LeaveType.LeaveTypeName).ToListAsync();
    }

    public async Task<IEnumerable<LeaveBalance>> GetByTenantIdAsync(int tenantId, int? year = null)
    {
        IQueryable<LeaveBalance> query = _dbSet
            .Where(lb => lb.TenantId == tenantId && lb.IsActive)
            .Include(lb => lb.LeaveType)
            .Include(lb => lb.Staff);

        if (year.HasValue)
        {
            query = query.Where(lb => lb.Year == year.Value);
        }

        return await query.OrderBy(lb => lb.Staff.Name).ThenBy(lb => lb.LeaveType.LeaveTypeName).ToListAsync();
    }

    public async Task<bool> UpdateUsedDaysAsync(int leaveBalanceId, decimal usedDays)
    {
        var leaveBalance = await _dbSet.FindAsync(leaveBalanceId);
        if (leaveBalance == null)
        {
            return false;
        }

        leaveBalance.UsedDays = usedDays;
        leaveBalance.RemainingDays = leaveBalance.TotalDays - usedDays;
        leaveBalance.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}

