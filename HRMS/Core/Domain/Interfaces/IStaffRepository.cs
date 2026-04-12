using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IStaffRepository : IRepository<Staff>
{
    Task<Staff?> GetByIdWithDetailsAsync(int staffId, int tenantId);
    Task<IEnumerable<Staff>> GetByTenantIdAsync(int tenantId);
    Task<bool> EmailExistsAsync(string email, int tenantId, int? excludeStaffId = null);
    Task<IEnumerable<Staff>> SearchAsync(int tenantId, string? searchTerm = null, string? division = null, string? department = null);
    Task<Staff?> GetLastByTenantAsync(int tenantId);

    // Stored procedure methods
    Task<(IEnumerable<StaffListSpDto> Items, int TotalCount)> GetStaffListSpAsync(GetStaffListRequest request);
}
