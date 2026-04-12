using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Application.Interfaces;

public interface IMasterDropdownService
{
    Task<IEnumerable<MasterDropdown>> GetByCategoryAsync(string category, int tenantId);
    Task<IEnumerable<MasterDropdown>> GetAllAsync(int tenantId);
}

