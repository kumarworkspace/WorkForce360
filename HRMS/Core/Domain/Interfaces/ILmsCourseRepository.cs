using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ILmsCourseRepository : IRepository<LmsCourse>
{
    Task<(IEnumerable<LmsCourse> Items, int TotalCount)> GetPagedAsync(int tenantId, string? search, int? courseTypeId, int? difficultyId, int pageNumber, int pageSize);
    Task<LmsCourse?> GetByIdAsync(int courseId, int tenantId);
    Task<IEnumerable<LmsCourse>> GetAllActiveAsync(int tenantId);
}
