using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ILeaveTypeMasterRepository : IRepository<LeaveTypeMaster>
{
    Task<IEnumerable<LeaveTypeMaster>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<bool> LeaveTypeNameExistsAsync(string leaveTypeName, int tenantId, int? excludeLeaveTypeId = null);
}





