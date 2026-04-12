using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<Permission?> GetByIdAsync(int permissionId, int tenantId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.PermissionId == permissionId && p.TenantId == tenantId);
    }

    public async Task<IEnumerable<Permission>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        var query = _dbSet.Where(p => p.TenantId == tenantId);
        
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }
        
        return await query.OrderBy(p => p.ModuleName).ToListAsync();
    }

    public async Task<Permission?> GetByModuleNameAsync(string moduleName, int tenantId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.ModuleName.ToLower() == moduleName.ToLower() && p.TenantId == tenantId);
    }

    public async Task<bool> ModuleNameExistsAsync(string moduleName, int tenantId, int? excludePermissionId = null)
    {
        var query = _dbSet.Where(p => p.ModuleName.ToLower() == moduleName.ToLower() && p.TenantId == tenantId);
        
        if (excludePermissionId.HasValue)
        {
            query = query.Where(p => p.PermissionId != excludePermissionId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<bool> IsPermissionMappedToRolesAsync(int permissionId, int tenantId)
    {
        return await _context.Set<RolePermission>()
            .AnyAsync(rp => rp.PermissionId == permissionId && rp.TenantId == tenantId && rp.IsActive);
    }
}





