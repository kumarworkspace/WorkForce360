using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, int tenantId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId && a.TenantId == tenantId && a.IsActive)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByTenantIdAsync(int tenantId)
    {
        return await _dbSet
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }
}
