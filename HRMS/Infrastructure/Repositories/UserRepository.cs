using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        // Don't filter by IsActive here - let the service layer handle validation
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsInTenantAsync(string email, int tenantId, int? excludeUserId = null)
    {
        // Check if email exists for active users only (IsActive = 1)
        var emailLower = email.ToLower().Trim();
        var query = _dbSet.Where(u => u.Email.ToLower().Trim() == emailLower && u.TenantId == tenantId && u.IsActive);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<IEnumerable<User>> GetAllUsers()
    {
        return await _dbSet.ToListAsync();
    }

}
