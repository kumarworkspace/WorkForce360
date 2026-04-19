using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> GetPagedAsync(int tenantId, int pageNumber, int pageSize, string? searchTerm, string? actionType, DateTime? startDate, DateTime? endDate, string? module = null);
    Task<AuditLogDto?> GetByIdAsync(int auditId, int tenantId);
}





