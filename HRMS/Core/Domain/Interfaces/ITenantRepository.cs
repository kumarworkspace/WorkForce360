using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByNameAsync(string companyName);
    Task<bool> TenantNameExistsAsync(string companyName);
    Task<IEnumerable<Tenant>> GetAllActiveTenantsAsync();
}


