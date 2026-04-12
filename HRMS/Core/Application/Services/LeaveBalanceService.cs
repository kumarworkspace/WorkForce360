using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class LeaveBalanceService : ILeaveBalanceService
{
    private readonly ILogger<LeaveBalanceService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveBalanceService(ILogger<LeaveBalanceService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveBalanceDto?> GetByStaffAndLeaveTypeAsync(int staffId, int leaveTypeId, int year, int tenantId)
    {
        try
        {
            var leaveBalance = await _unitOfWork.LeaveBalance.GetByStaffAndLeaveTypeAsync(staffId, leaveTypeId, year, tenantId);
            if (leaveBalance == null)
            {
                return null;
            }

            return MapToDto(leaveBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave balance for staff: {StaffId}, leaveType: {LeaveTypeId}, year: {Year}, tenant: {TenantId}", 
                staffId, leaveTypeId, year, tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<LeaveBalanceDto>> GetByStaffIdAsync(int staffId, int tenantId, int? year = null)
    {
        try
        {
            var leaveBalances = await _unitOfWork.LeaveBalance.GetByStaffIdAsync(staffId, tenantId, year);
            return leaveBalances.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave balances for staff: {StaffId}, tenant: {TenantId}", staffId, tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<LeaveBalanceDto>> GetByTenantIdAsync(int tenantId, int? year = null)
    {
        try
        {
            var leaveBalances = await _unitOfWork.LeaveBalance.GetByTenantIdAsync(tenantId, year);
            return leaveBalances.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave balances for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<LeaveBalanceDto> InitializeLeaveBalanceAsync(CreateLeaveBalanceDto request, int tenantId, int userId)
    {
        try
        {
            // Check if balance already exists
            var existing = await _unitOfWork.LeaveBalance.GetByStaffAndLeaveTypeAsync(
                request.StaffId, request.LeaveTypeId, request.Year, tenantId);

            if (existing != null)
            {
                throw new InvalidOperationException($"Leave balance already exists for staff {request.StaffId}, leave type {request.LeaveTypeId}, year {request.Year}");
            }

            var leaveBalance = new LeaveBalance
            {
                TenantId = tenantId,
                StaffId = request.StaffId,
                LeaveTypeId = request.LeaveTypeId,
                TotalDays = request.TotalDays,
                UsedDays = 0,
                RemainingDays = request.TotalDays,
                Year = request.Year,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.LeaveBalance.AddAsync(leaveBalance);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Initialized leave balance for staff: {StaffId}, leaveType: {LeaveTypeId}, year: {Year}, totalDays: {TotalDays}",
                request.StaffId, request.LeaveTypeId, request.Year, request.TotalDays);

            return MapToDto(leaveBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing leave balance");
            throw;
        }
    }

    public async Task<LeaveBalanceDto> UpdateUsedDaysAsync(int leaveBalanceId, decimal usedDays, int tenantId, int userId)
    {
        try
        {
            var leaveBalance = await _unitOfWork.LeaveBalance.GetByIdAsync(leaveBalanceId);
            if (leaveBalance == null || leaveBalance.TenantId != tenantId)
            {
                throw new InvalidOperationException("Leave balance not found.");
            }

            leaveBalance.UsedDays = usedDays;
            leaveBalance.RemainingDays = leaveBalance.TotalDays - usedDays;
            leaveBalance.UpdatedBy = userId;
            leaveBalance.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.LeaveBalance.UpdateAsync(leaveBalance);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(leaveBalance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leave balance: {LeaveBalanceId}", leaveBalanceId);
            throw;
        }
    }

    public async Task<decimal> GetRemainingDaysAsync(int staffId, int leaveTypeId, int year, int tenantId)
    {
        try
        {
            var leaveBalance = await _unitOfWork.LeaveBalance.GetByStaffAndLeaveTypeAsync(staffId, leaveTypeId, year, tenantId);
            if (leaveBalance == null)
            {
                // Return 0 if balance doesn't exist
                return 0;
            }

            return leaveBalance.RemainingDays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting remaining days for staff: {StaffId}, leaveType: {LeaveTypeId}, year: {Year}", 
                staffId, leaveTypeId, year);
            throw;
        }
    }

    public async Task<bool> DeductLeaveDaysAsync(int staffId, int leaveTypeId, decimal days, int year, int tenantId, int userId)
    {
        try
        {
            var leaveBalance = await _unitOfWork.LeaveBalance.GetByStaffAndLeaveTypeAsync(staffId, leaveTypeId, year, tenantId);
            
            if (leaveBalance == null)
            {
                // Initialize balance if it doesn't exist
                var leaveType = await _unitOfWork.LeaveTypeMaster.GetByIdAsync(leaveTypeId);
                if (leaveType == null || leaveType.TenantId != tenantId)
                {
                    throw new InvalidOperationException("Leave type not found.");
                }

                var createRequest = new CreateLeaveBalanceDto
                {
                    StaffId = staffId,
                    LeaveTypeId = leaveTypeId,
                    TotalDays = leaveType.MaxDaysPerYear,
                    Year = year
                };
                await InitializeLeaveBalanceAsync(createRequest, tenantId, userId);
                leaveBalance = await _unitOfWork.LeaveBalance.GetByStaffAndLeaveTypeAsync(staffId, leaveTypeId, year, tenantId);
            }

            if (leaveBalance == null)
            {
                throw new InvalidOperationException("Failed to create or retrieve leave balance.");
            }

            if (leaveBalance.RemainingDays < days)
            {
                throw new InvalidOperationException($"Insufficient leave balance. Remaining: {leaveBalance.RemainingDays}, Requested: {days}");
            }

            leaveBalance.UsedDays += days;
            leaveBalance.RemainingDays = leaveBalance.TotalDays - leaveBalance.UsedDays;
            leaveBalance.UpdatedBy = userId;
            leaveBalance.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.LeaveBalance.UpdateAsync(leaveBalance);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deducted {Days} days from leave balance for staff: {StaffId}, leaveType: {LeaveTypeId}, year: {Year}",
                days, staffId, leaveTypeId, year);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deducting leave days");
            throw;
        }
    }

    public async Task<bool> RestoreLeaveDaysAsync(int staffId, int leaveTypeId, decimal days, int year, int tenantId, int userId)
    {
        try
        {
            var leaveBalance = await _unitOfWork.LeaveBalance.GetByStaffAndLeaveTypeAsync(staffId, leaveTypeId, year, tenantId);
            if (leaveBalance == null)
            {
                throw new InvalidOperationException("Leave balance not found.");
            }

            leaveBalance.UsedDays = Math.Max(0, leaveBalance.UsedDays - days);
            leaveBalance.RemainingDays = leaveBalance.TotalDays - leaveBalance.UsedDays;
            leaveBalance.UpdatedBy = userId;
            leaveBalance.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.LeaveBalance.UpdateAsync(leaveBalance);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Restored {Days} days to leave balance for staff: {StaffId}, leaveType: {LeaveTypeId}, year: {Year}",
                days, staffId, leaveTypeId, year);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring leave days");
            throw;
        }
    }

    private LeaveBalanceDto MapToDto(LeaveBalance leaveBalance)
    {
        return new LeaveBalanceDto
        {
            LeaveBalanceId = leaveBalance.LeaveBalanceId,
            StaffId = leaveBalance.StaffId,
            StaffName = leaveBalance.Staff?.Name ?? string.Empty,
            LeaveTypeId = leaveBalance.LeaveTypeId,
            LeaveTypeName = leaveBalance.LeaveType?.LeaveTypeName ?? string.Empty,
            TotalDays = leaveBalance.TotalDays,
            UsedDays = leaveBalance.UsedDays,
            RemainingDays = leaveBalance.RemainingDays,
            Year = leaveBalance.Year,
            IsActive = leaveBalance.IsActive
        };
    }
}





