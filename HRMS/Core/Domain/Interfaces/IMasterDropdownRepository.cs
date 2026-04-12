using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IMasterDropdownRepository : IRepository<MasterDropdown>
{
    Task<IEnumerable<MasterDropdown>> GetByCategoryAsync(string category, int tenantId);
    Task<IEnumerable<MasterDropdown>> GetActiveByTenantIdAsync(int tenantId);
    Task<MasterDropdown?> GetByCategoryAndNameAsync(string category, string name, int tenantId);
}
