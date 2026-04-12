using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ILeaveBalanceRepository : IRepository<LeaveBalance>
{
    Task<LeaveBalance?> GetByStaffAndLeaveTypeAsync(int staffId, int leaveTypeId, int year, int tenantId);
    Task<IEnumerable<LeaveBalance>> GetByStaffIdAsync(int staffId, int tenantId, int? year = null);
    Task<IEnumerable<LeaveBalance>> GetByTenantIdAsync(int tenantId, int? year = null);
    Task<bool> UpdateUsedDaysAsync(int leaveBalanceId, decimal usedDays);
}





