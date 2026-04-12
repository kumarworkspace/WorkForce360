using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class MasterDropdownRepository : Repository<MasterDropdown>, IMasterDropdownRepository
{
    public MasterDropdownRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MasterDropdown>> GetByCategoryAsync(string category, int tenantId)
    {
        try
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.Category == category && m.TenantId == tenantId && m.IsActive)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log and rethrow to see the actual error
            System.Diagnostics.Debug.WriteLine($"Error in GetByCategoryAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<IEnumerable<MasterDropdown>> GetActiveByTenantIdAsync(int tenantId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.IsActive)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<MasterDropdown?> GetByCategoryAndNameAsync(string category, string name, int tenantId)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(m => 
                m.Category == category && 
                m.Name == name && 
                m.TenantId == tenantId && 
                m.IsActive);
    }
}
