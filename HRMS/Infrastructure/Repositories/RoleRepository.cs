using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByIdAsync(int roleId, int tenantId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.TenantId == tenantId && r.IsActive);
    }

    public async Task<IEnumerable<Role>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        var query = _dbSet.Where(r => r.TenantId == tenantId);
        
        if (!includeInactive)
        {
            query = query.Where(r => r.IsActive);
        }
        
        return await query.OrderBy(r => r.RoleName).ToListAsync();
    }

    public async Task<bool> RoleNameExistsAsync(string roleName, int tenantId, int? excludeRoleId = null)
    {
        var query = _dbSet.Where(r => r.RoleName.ToLower() == roleName.ToLower() && r.TenantId == tenantId);
        
        if (excludeRoleId.HasValue)
        {
            query = query.Where(r => r.RoleId != excludeRoleId.Value);
        }
        
        return await query.AnyAsync();
    }

    public async Task<bool> IsRoleAssignedToUsersAsync(int roleId, int tenantId)
    {
        return await _context.Set<UserRole>()
            .AnyAsync(ur => ur.RoleId == roleId && ur.TenantId == tenantId && ur.IsActive);
    }
}





