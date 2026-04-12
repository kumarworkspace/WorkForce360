using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IUserRoleRepository : IRepository<UserRole>
{
    Task<IEnumerable<UserRole>> GetByUserIdAsync(int userId, int tenantId);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(int roleId, int tenantId);
    Task<UserRole?> GetByUserAndRoleAsync(int userId, int roleId, int tenantId);
    Task<bool> ExistsAsync(int userId, int roleId, int tenantId);
    Task DeleteByUserIdAsync(int userId, int tenantId);
    Task DeleteByRoleIdAsync(int roleId, int tenantId);
}

