using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface IHolidayMasterRepository : IRepository<HolidayMaster>
{
    Task<IEnumerable<HolidayMaster>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<IEnumerable<HolidayMaster>> GetByDateRangeAsync(int tenantId, DateTime startDate, DateTime endDate);
    Task<bool> HolidayDateExistsAsync(DateTime holidayDate, int tenantId, int? excludeHolidayId = null);
}





