using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class RolePermissionRepository : Repository<RolePermission>, IRolePermissionRepository
{
    public RolePermissionRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<RolePermission?> GetByRoleAndPermissionAsync(int roleId, int permissionId, int tenantId)
    {
        return await _dbSet
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.TenantId == tenantId);
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId, int tenantId)
    {
        return await _dbSet
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId && rp.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(int permissionId, int tenantId)
    {
        return await _dbSet
            .Include(rp => rp.Role)
            .Where(rp => rp.PermissionId == permissionId && rp.TenantId == tenantId && rp.IsActive)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int roleId, int permissionId, int tenantId)
    {
        return await _dbSet
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.TenantId == tenantId);
    }

    public async Task DeleteByRoleIdAsync(int roleId, int tenantId)
    {
        var rolePermissions = await _dbSet
            .Where(rp => rp.RoleId == roleId && rp.TenantId == tenantId)
            .ToListAsync();
        
        _dbSet.RemoveRange(rolePermissions);
    }
}





