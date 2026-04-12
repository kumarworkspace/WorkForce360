using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class HolidayMasterService : IHolidayMasterService
{
    private readonly ILogger<HolidayMasterService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HolidayMasterService(ILogger<HolidayMasterService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<HolidayMasterDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        try
        {
            var holidays = await _unitOfWork.HolidayMaster.GetByTenantIdAsync(tenantId, includeInactive);
            return holidays.Select(h => new HolidayMasterDto
            {
                HolidayId = h.HolidayId,
                HolidayDate = h.HolidayDate,
                HolidayName = h.HolidayName,
                IsActive = h.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting holidays for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<IEnumerable<HolidayMasterDto>> GetByDateRangeAsync(int tenantId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var holidays = await _unitOfWork.HolidayMaster.GetByDateRangeAsync(tenantId, startDate, endDate);
            return holidays.Select(h => new HolidayMasterDto
            {
                HolidayId = h.HolidayId,
                HolidayDate = h.HolidayDate,
                HolidayName = h.HolidayName,
                IsActive = h.IsActive
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting holidays for date range, tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<HolidayMasterDto?> GetByIdAsync(int holidayId, int tenantId)
    {
        try
        {
            var holiday = await _unitOfWork.HolidayMaster.GetByIdAsync(holidayId);
            if (holiday == null || holiday.TenantId != tenantId)
            {
                return null;
            }

            return new HolidayMasterDto
            {
                HolidayId = holiday.HolidayId,
                HolidayDate = holiday.HolidayDate,
                HolidayName = holiday.HolidayName,
                IsActive = holiday.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting holiday: {HolidayId}, tenant: {TenantId}", holidayId, tenantId);
            throw;
        }
    }

    public async Task<PagedResult<HolidayMasterDto>> GetPagedAsync(int tenantId, int pageNumber, int pageSize, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null)
    {
        try
        {
            // Get all holidays for the tenant
            var allHolidays = await _unitOfWork.HolidayMaster.GetByTenantIdAsync(tenantId, includeInactive: true);
            var query = allHolidays.AsQueryable();
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(h => 
                    h.HolidayName.ToLower().Contains(searchLower) ||
                    h.HolidayDate.ToString("MMM dd, yyyy").ToLower().Contains(searchLower));
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                query = query.Where(h => h.HolidayDate >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(h => h.HolidayDate <= endDate.Value.Date);
            }

            // Apply active status filter
            if (isActive.HasValue)
            {
                query = query.Where(h => h.IsActive == isActive.Value);
            }

            // Get total count before pagination
            var totalCount = query.Count();

            // Apply pagination and ordering
            var items = query
                .OrderBy(h => h.HolidayDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HolidayMasterDto
                {
                    HolidayId = h.HolidayId,
                    HolidayDate = h.HolidayDate,
                    HolidayName = h.HolidayName,
                    IsActive = h.IsActive
                })
                .ToList();

            return new PagedResult<HolidayMasterDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged holidays for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<HolidayMasterDto> CreateAsync(CreateHolidayMasterDto request, int tenantId, int userId)
    {
        try
        {
            // Validate unique date
            if (await _unitOfWork.HolidayMaster.HolidayDateExistsAsync(request.HolidayDate, tenantId))
            {
                throw new InvalidOperationException($"Holiday for date '{request.HolidayDate:yyyy-MM-dd}' already exists for this tenant.");
            }

            var holiday = new HolidayMaster
            {
                TenantId = tenantId,
                HolidayDate = request.HolidayDate.Date,
                HolidayName = request.HolidayName,
                IsActive = true,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.HolidayMaster.AddAsync(holiday);
            await _unitOfWork.SaveChangesAsync();

            // Log audit (don't let audit logging failure prevent holiday creation)
            try
            {
                await LogAuditAsync(tenantId, userId, ActionType.Create, 
                    $"Created holiday: {request.HolidayName} on {request.HolidayDate:yyyy-MM-dd}");
            }
            catch (Exception auditEx)
            {
                _logger.LogWarning(auditEx, "Failed to log audit entry for holiday creation, but holiday was created successfully");
            }

            return new HolidayMasterDto
            {
                HolidayId = holiday.HolidayId,
                HolidayDate = holiday.HolidayDate,
                HolidayName = holiday.HolidayName,
                IsActive = holiday.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating holiday for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<HolidayMasterDto> UpdateAsync(UpdateHolidayMasterDto request, int tenantId, int userId)
    {
        try
        {
            var holiday = await _unitOfWork.HolidayMaster.GetByIdAsync(request.HolidayId);
            if (holiday == null || holiday.TenantId != tenantId)
            {
                throw new InvalidOperationException("Holiday not found.");
            }

            // Validate unique date (excluding current)
            if (await _unitOfWork.HolidayMaster.HolidayDateExistsAsync(request.HolidayDate, tenantId, request.HolidayId))
            {
                throw new InvalidOperationException($"Holiday for date '{request.HolidayDate:yyyy-MM-dd}' already exists for this tenant.");
            }

            holiday.HolidayDate = request.HolidayDate.Date;
            holiday.HolidayName = request.HolidayName;
            holiday.IsActive = request.IsActive;
            // HolidayMaster doesn't have UpdatedBy/UpdatedDate columns

            await _unitOfWork.HolidayMaster.UpdateAsync(holiday);
            await _unitOfWork.SaveChangesAsync();

            // Log audit (don't let audit logging failure prevent holiday update)
            try
            {
                await LogAuditAsync(tenantId, userId, ActionType.Update, 
                    $"Updated holiday: {request.HolidayName} on {request.HolidayDate:yyyy-MM-dd}");
            }
            catch (Exception auditEx)
            {
                _logger.LogWarning(auditEx, "Failed to log audit entry for holiday update, but holiday was updated successfully");
            }

            return new HolidayMasterDto
            {
                HolidayId = holiday.HolidayId,
                HolidayDate = holiday.HolidayDate,
                HolidayName = holiday.HolidayName,
                IsActive = holiday.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating holiday: {HolidayId}, tenant: {TenantId}", request.HolidayId, tenantId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int holidayId, int tenantId, int userId)
    {
        try
        {
            var holiday = await _unitOfWork.HolidayMaster.GetByIdAsync(holidayId);
            if (holiday == null || holiday.TenantId != tenantId)
            {
                throw new InvalidOperationException("Holiday not found.");
            }

            // Soft delete
            holiday.IsActive = false;
            // HolidayMaster doesn't have UpdatedBy/UpdatedDate columns

            await _unitOfWork.HolidayMaster.UpdateAsync(holiday);
            await _unitOfWork.SaveChangesAsync();

            // Log audit (don't let audit logging failure prevent holiday deletion)
            try
            {
                await LogAuditAsync(tenantId, userId, ActionType.Delete, 
                    $"Deleted holiday: {holiday.HolidayName} on {holiday.HolidayDate:yyyy-MM-dd}");
            }
            catch (Exception auditEx)
            {
                _logger.LogWarning(auditEx, "Failed to log audit entry for holiday deletion, but holiday was deleted successfully");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting holiday: {HolidayId}, tenant: {TenantId}", holidayId, tenantId);
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
            _logger.LogWarning(ex, "Failed to log audit entry for holiday operation");
        }
    }
}

