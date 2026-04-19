using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IMasterCategoryRepository : IRepository<MasterCategory>
{
    Task<IEnumerable<MasterCategory>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<MasterCategory?> GetByCodeAsync(string categoryCode, int tenantId);
    Task<MasterCategory?> GetByIdAsync(int categoryId, int tenantId);
}
