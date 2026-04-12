using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IEducationDetailRepository : IRepository<EducationDetail>
{
    Task<IEnumerable<EducationDetail>> GetByStaffIdAsync(int staffId, int tenantId);
}

