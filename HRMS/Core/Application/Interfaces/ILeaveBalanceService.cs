using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ILeaveBalanceService
{
    Task<LeaveBalanceDto?> GetByStaffAndLeaveTypeAsync(int staffId, int leaveTypeId, int year, int tenantId);
    Task<IEnumerable<LeaveBalanceDto>> GetByStaffIdAsync(int staffId, int tenantId, int? year = null);
    Task<IEnumerable<LeaveBalanceDto>> GetByTenantIdAsync(int tenantId, int? year = null);
    Task<LeaveBalanceDto> InitializeLeaveBalanceAsync(CreateLeaveBalanceDto request, int tenantId, int userId);
    Task<LeaveBalanceDto> UpdateUsedDaysAsync(int leaveBalanceId, decimal usedDays, int tenantId, int userId);
    Task<decimal> GetRemainingDaysAsync(int staffId, int leaveTypeId, int year, int tenantId);
    Task<bool> DeductLeaveDaysAsync(int staffId, int leaveTypeId, decimal days, int year, int tenantId, int userId);
    Task<bool> RestoreLeaveDaysAsync(int staffId, int leaveTypeId, decimal days, int year, int tenantId, int userId);
}





