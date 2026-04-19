using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ILmsModuleRepository : IRepository<LmsModule>
{
    Task<IEnumerable<LmsModule>> GetByCourseIdAsync(int courseId);
    Task<LmsModule?> GetByIdAsync(int moduleId);
}
