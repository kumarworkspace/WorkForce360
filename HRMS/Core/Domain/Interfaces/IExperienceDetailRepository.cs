using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IExperienceDetailRepository : IRepository<ExperienceDetail>
{
    Task<IEnumerable<ExperienceDetail>> GetByStaffIdAsync(int staffId, int tenantId);
}

