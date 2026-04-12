using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, int tenantId);
    Task<IEnumerable<AuditLog>> GetByTenantIdAsync(int tenantId);
}
