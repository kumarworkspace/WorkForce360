using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IMasterValueRepository : IRepository<MasterValue>
{
    Task<IEnumerable<MasterValue>> GetByCategoryIdAsync(int categoryId, bool includeInactive = false);
    Task<IEnumerable<MasterValue>> GetByCategoryCodeAsync(string categoryCode, int tenantId);
    Task<MasterValue?> GetByIdAsync(int valueId, int tenantId);
}
