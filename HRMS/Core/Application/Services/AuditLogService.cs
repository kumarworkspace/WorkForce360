using HRMS.Core.Application.DTOs;
using HRMS.Core.Application.Interfaces;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRMS.Core.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ILogger<AuditLogService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(ILogger<AuditLogService> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(int tenantId, int pageNumber, int pageSize, string? searchTerm, string? actionType, DateTime? startDate, DateTime? endDate, string? module = null)
    {
        try
        {
            // Get all audit logs and filter
            var allAuditLogs = await _unitOfWork.AuditLogs.GetAllAsync();
            var query = allAuditLogs
                .Where(a => a.TenantId == tenantId && a.IsActive)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(a =>
                    (a.Description != null && a.Description.ToLower().Contains(searchLower)) ||
                    (a.IPAddress != null && a.IPAddress.ToLower().Contains(searchLower)) ||
                    (a.Module != null && a.Module.ToLower().Contains(searchLower))
                );
            }

            // Apply action type filter
            if (!string.IsNullOrWhiteSpace(actionType) && Enum.TryParse<ActionType>(actionType, out var parsedActionType))
            {
                query = query.Where(a => a.ActionType == parsedActionType);
            }

            // Apply module filter
            if (!string.IsNullOrWhiteSpace(module))
            {
                var moduleLower = module.ToLower();
                query = query.Where(a => a.Module != null && a.Module.ToLower() == moduleLower);
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                query = query.Where(a => a.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.CreatedDate <= endDate.Value.AddDays(1).AddTicks(-1)); // End of day
            }

            var totalCount = query.Count();

            // Get audit logs with pagination
            var auditLogs = query
                .OrderByDescending(a => a.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get user details for audit logs
            var userIds = auditLogs.Where(a => a.UserId.HasValue).Select(a => a.UserId!.Value).Distinct().ToList();
            var users = new Dictionary<int, User>();
            foreach (var userId in userIds)
            {
                try
                {
                    var user = await _unitOfWork.User.GetByIdAsync(userId);
                    if (user != null)
                    {
                        users[userId] = user;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading user {UserId} for audit log", userId);
                }
            }

            // Get creator details
            var creatorIds = auditLogs.Where(a => a.CreatedBy.HasValue).Select(a => a.CreatedBy!.Value).Distinct().ToList();
            var creators = new Dictionary<int, User>();
            foreach (var creatorId in creatorIds)
            {
                try
                {
                    var creator = await _unitOfWork.User.GetByIdAsync(creatorId);
                    if (creator != null)
                    {
                        creators[creatorId] = creator;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error loading creator {CreatorId} for audit log", creatorId);
                }
            }

            // Map to DTOs
            var auditLogDtos = auditLogs.Select(a =>
            {
                var user = a.UserId.HasValue && users.ContainsKey(a.UserId.Value) ? users[a.UserId.Value] : null;
                var creator = a.CreatedBy.HasValue && creators.ContainsKey(a.CreatedBy.Value) ? creators[a.CreatedBy.Value] : null;

                return new AuditLogDto
                {
                    AuditId = a.AuditId,
                    TenantId = a.TenantId,
                    UserId = a.UserId,
                    UserName = user?.FullName,
                    UserEmail = user?.Email,
                    UserRole = user?.Role,
                    ActionType = a.ActionType.ToString(),
                    Module = a.Module,
                    RecordId = a.RecordId,
                    Description = a.Description,
                    IPAddress = a.IPAddress,
                    CreatedDate = a.CreatedDate,
                    CreatedBy = a.CreatedBy,
                    CreatedByName = creator?.FullName
                };
            }).ToList();

            return new PagedResult<AuditLogDto>
            {
                Items = auditLogDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged audit logs for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<AuditLogDto?> GetByIdAsync(int auditId, int tenantId)
    {
        try
        {
            var auditLog = await _unitOfWork.AuditLogs.GetByIdAsync(auditId);
            if (auditLog == null || auditLog.TenantId != tenantId || !auditLog.IsActive)
            {
                return null;
            }

            var user = auditLog.UserId.HasValue ? await _unitOfWork.User.GetByIdAsync(auditLog.UserId.Value) : null;
            var creator = auditLog.CreatedBy.HasValue ? await _unitOfWork.User.GetByIdAsync(auditLog.CreatedBy.Value) : null;

            return new AuditLogDto
            {
                AuditId = auditLog.AuditId,
                TenantId = auditLog.TenantId,
                UserId = auditLog.UserId,
                UserName = user?.FullName,
                UserEmail = user?.Email,
                UserRole = user?.Role,
                ActionType = auditLog.ActionType.ToString(),
                Module = auditLog.Module,
                RecordId = auditLog.RecordId,
                Description = auditLog.Description,
                IPAddress = auditLog.IPAddress,
                CreatedDate = auditLog.CreatedDate,
                CreatedBy = auditLog.CreatedBy,
                CreatedByName = creator?.FullName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit log {AuditId} for tenant {TenantId}", auditId, tenantId);
            throw;
        }
    }
}

