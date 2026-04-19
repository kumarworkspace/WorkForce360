using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ILearningPathRepository : IRepository<LearningPath>
{
    Task<IEnumerable<LearningPath>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<LearningPath?> GetByIdAsync(int pathId, int tenantId);
}
