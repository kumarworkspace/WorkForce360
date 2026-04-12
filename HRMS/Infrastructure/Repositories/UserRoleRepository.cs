using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId, int tenantId)
    {
        return await _dbSet
            .Include(ur => ur.Role)
            // Removed .Include(ur => ur.User) to avoid shadow property issues
            // User details are fetched separately in the service layer
            .Where(ur => ur.UserId == userId && ur.TenantId == tenantId && ur.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId, int tenantId)
    {
        return await _dbSet
            .Include(ur => ur.Role)
            // Removed .Include(ur => ur.User) to avoid shadow property issues
            // User details are fetched separately in the service layer
            .Where(ur => ur.RoleId == roleId && ur.TenantId == tenantId && ur.IsActive)
            .ToListAsync();
    }

    public async Task<UserRole?> GetByUserAndRoleAsync(int userId, int roleId, int tenantId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.TenantId == tenantId);
    }

    public async Task<bool> ExistsAsync(int userId, int roleId, int tenantId)
    {
        return await _dbSet
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.TenantId == tenantId);
    }

    public async Task DeleteByUserIdAsync(int userId, int tenantId)
    {
        var userRoles = await _dbSet
            .Where(ur => ur.UserId == userId && ur.TenantId == tenantId)
            .ToListAsync();
        
        _dbSet.RemoveRange(userRoles);
    }

    public async Task DeleteByRoleIdAsync(int roleId, int tenantId)
    {
        var userRoles = await _dbSet
            .Where(ur => ur.RoleId == roleId && ur.TenantId == tenantId)
            .ToListAsync();
        
        _dbSet.RemoveRange(userRoles);
    }
}

