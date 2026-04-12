using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface IHolidayMasterService
{
    Task<IEnumerable<HolidayMasterDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<IEnumerable<HolidayMasterDto>> GetByDateRangeAsync(int tenantId, DateTime startDate, DateTime endDate);
    Task<HolidayMasterDto?> GetByIdAsync(int holidayId, int tenantId);
    Task<PagedResult<HolidayMasterDto>> GetPagedAsync(int tenantId, int pageNumber, int pageSize, string? searchTerm = null, DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null);
    Task<HolidayMasterDto> CreateAsync(CreateHolidayMasterDto request, int tenantId, int userId);
    Task<HolidayMasterDto> UpdateAsync(UpdateHolidayMasterDto request, int tenantId, int userId);
    Task<bool> DeleteAsync(int holidayId, int tenantId, int userId);
}

