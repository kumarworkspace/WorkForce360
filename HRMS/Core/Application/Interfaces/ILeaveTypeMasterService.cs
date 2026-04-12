using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ILeaveTypeMasterService
{
    Task<IEnumerable<LeaveTypeMasterDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<LeaveTypeMasterDto?> GetByIdAsync(int leaveTypeId, int tenantId);
    Task<LeaveTypeMasterDto> CreateAsync(CreateLeaveTypeMasterDto request, int tenantId, int userId);
    Task<LeaveTypeMasterDto> UpdateAsync(UpdateLeaveTypeMasterDto request, int tenantId, int userId);
    Task<bool> DeleteAsync(int leaveTypeId, int tenantId, int userId);
}





