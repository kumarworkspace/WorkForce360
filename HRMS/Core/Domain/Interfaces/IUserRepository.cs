using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> EmailExistsInTenantAsync(string email, int tenantId, int? excludeUserId = null);
    Task<IEnumerable<User>> GetAllUsers();
}
