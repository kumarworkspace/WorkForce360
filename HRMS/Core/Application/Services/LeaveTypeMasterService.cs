using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class LeaveTypeMasterService : ILeaveTypeMasterService
{
    private readonly ILogger<LeaveTypeMasterService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveTypeMasterService(ILogger<LeaveTypeMasterService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<LeaveTypeMasterDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        try
        {
            var leaveTypes = await _unitOfWork.LeaveTypeMaster.GetByTenantIdAsync(tenantId, includeInactive);
            return leaveTypes.Select(lt => new LeaveTypeMasterDto
            {
                LeaveTypeId = lt.LeaveTypeId,
                LeaveTypeName = lt.LeaveTypeName,
                MaxDaysPerYear = lt.MaxDaysPerYear,
                IsPaid = lt.IsPaid,
                IsActive = lt.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave types for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<LeaveTypeMasterDto?> GetByIdAsync(int leaveTypeId, int tenantId)
    {
        try
        {
            var leaveType = await _unitOfWork.LeaveTypeMaster.GetByIdAsync(leaveTypeId);
            if (leaveType == null || leaveType.TenantId != tenantId)
            {
                return null;
            }

            return new LeaveTypeMasterDto
            {
                LeaveTypeId = leaveType.LeaveTypeId,
                LeaveTypeName = leaveType.LeaveTypeName,
                MaxDaysPerYear = leaveType.MaxDaysPerYear,
                IsPaid = leaveType.IsPaid,
                IsActive = leaveType.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leave type: {LeaveTypeId}, tenant: {TenantId}", leaveTypeId, tenantId);
            throw;
        }
    }

    public async Task<LeaveTypeMasterDto> CreateAsync(CreateLeaveTypeMasterDto request, int tenantId, int userId)
    {
        try
        {
            // Validate unique name
            if (await _unitOfWork.LeaveTypeMaster.LeaveTypeNameExistsAsync(request.LeaveTypeName, tenantId))
            {
                throw new InvalidOperationException($"Leave type '{request.LeaveTypeName}' already exists for this tenant.");
            }

            var leaveType = new LeaveTypeMaster
            {
                TenantId = tenantId,
                LeaveTypeName = request.LeaveTypeName,
                MaxDaysPerYear = request.MaxDaysPerYear,
                IsPaid = request.IsPaid,
                IsActive = true,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.LeaveTypeMaster.AddAsync(leaveType);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Create, 
                $"Created leave type: {request.LeaveTypeName}");

            return new LeaveTypeMasterDto
            {
                LeaveTypeId = leaveType.LeaveTypeId,
                LeaveTypeName = leaveType.LeaveTypeName,
                MaxDaysPerYear = leaveType.MaxDaysPerYear,
                IsPaid = leaveType.IsPaid,
                IsActive = leaveType.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave type for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<LeaveTypeMasterDto> UpdateAsync(UpdateLeaveTypeMasterDto request, int tenantId, int userId)
    {
        try
        {
            var leaveType = await _unitOfWork.LeaveTypeMaster.GetByIdAsync(request.LeaveTypeId);
            if (leaveType == null || leaveType.TenantId != tenantId)
            {
                throw new InvalidOperationException("Leave type not found.");
            }

            // Validate unique name (excluding current)
            if (await _unitOfWork.LeaveTypeMaster.LeaveTypeNameExistsAsync(request.LeaveTypeName, tenantId, request.LeaveTypeId))
            {
                throw new InvalidOperationException($"Leave type '{request.LeaveTypeName}' already exists for this tenant.");
            }

            leaveType.LeaveTypeName = request.LeaveTypeName;
            leaveType.MaxDaysPerYear = request.MaxDaysPerYear;
            leaveType.IsPaid = request.IsPaid;
            leaveType.IsActive = request.IsActive;
            leaveType.UpdatedBy = userId.ToString();
            leaveType.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.LeaveTypeMaster.UpdateAsync(leaveType);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Update, 
                $"Updated leave type: {request.LeaveTypeName}");

            return new LeaveTypeMasterDto
            {
                LeaveTypeId = leaveType.LeaveTypeId,
                LeaveTypeName = leaveType.LeaveTypeName,
                MaxDaysPerYear = leaveType.MaxDaysPerYear,
                IsPaid = leaveType.IsPaid,
                IsActive = leaveType.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leave type: {LeaveTypeId}, tenant: {TenantId}", request.LeaveTypeId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int leaveTypeId, int tenantId, int userId)
    {
        try
        {
            var leaveType = await _unitOfWork.LeaveTypeMaster.GetByIdAsync(leaveTypeId);
            if (leaveType == null || leaveType.TenantId != tenantId)
            {
                throw new InvalidOperationException("Leave type not found.");
            }

            // Soft delete
            leaveType.IsActive = false;
            leaveType.UpdatedBy = userId.ToString();
            leaveType.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.LeaveTypeMaster.UpdateAsync(leaveType);
            await _unitOfWork.SaveChangesAsync();

            // Log audit
            await LogAuditAsync(tenantId, userId, ActionType.Delete, 
                $"Deleted leave type: {leaveType.LeaveTypeName}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting leave type: {LeaveTypeId}, tenant: {TenantId}", leaveTypeId, tenantId);
            throw;
        }
    }

    private async Task LogAuditAsync(int tenantId, int userId, ActionType actionType, string description)
    {
        try
        {
            var auditLog = new AuditLog
            {
                TenantId = tenantId,
                UserId = userId,
                ActionType = actionType,
                Description = description,
                IPAddress = null,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log audit entry for leave type operation");
        }
    }
}

