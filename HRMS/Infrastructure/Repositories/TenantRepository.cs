using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetByNameAsync(string companyName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.CompanyName == companyName);
    }

    public async Task<bool> TenantNameExistsAsync(string companyName)
    {
        return await _dbSet.AnyAsync(t => t.CompanyName == companyName);
    }

    public async Task<IEnumerable<Tenant>> GetAllActiveTenantsAsync()
    {
        return await _dbSet
            .Where(t => t.IsActive && !t.IsLocked)
            .ToListAsync();
    }
}


