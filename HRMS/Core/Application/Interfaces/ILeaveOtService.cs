using HRMS.Core.Application.DTOs;

namespace HRMS.Core.Application.Interfaces;

public interface ILeaveOtService
{
    Task<LeaveOtRequestDto> CreateRequestAsync(CreateLeaveOtRequestDto request, int tenantId, int userId, string? ipAddress);
    Task<LeaveOtRequestDto> UpdateRequestAsync(UpdateLeaveOtRequestDto request, int tenantId, int userId, string? ipAddress);
    Task<bool> ApproveRequestAsync(ApproveLeaveOtRequestDto request, int tenantId, int approverUserId, bool isHRApproval, string? ipAddress);
    Task<bool> CancelRequestAsync(int requestId, int tenantId, int userId, string? ipAddress);
    Task<IEnumerable<LeaveOtRequestDto>> GetByStaffIdAsync(int staffId, int tenantId);
    Task<IEnumerable<LeaveOtRequestDto>> GetPendingByReportingManagerIdAsync(int reportingManagerId, int tenantId);
    Task<IEnumerable<LeaveOtRequestDto>> GetPendingForHRAsync(int tenantId);
    Task<IEnumerable<LeaveOtRequestDto>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);
    Task<LeaveOtRequestDto?> GetByIdAsync(int requestId, int tenantId);
    Task<decimal> CalculateLeaveDaysAsync(DateTime fromDate, DateTime toDate, int tenantId);
    Task<decimal> CalculateOTHoursAsync(DateTime fromDate, DateTime toDate);

    // Stored procedure methods
    Task<PagedResult<LeaveRequestListSpDto>> GetLeaveRequestListSpAsync(GetLeaveRequestListRequest request);
    Task<IEnumerable<LeaveRequestListSpDto>> GetLeaveRequestByStaffSpAsync(int tenantId, int staffId, int? requestTypeId = null, int? year = null, bool isActive = true);
    Task<PagedResult<LeaveApprovalListSpDto>> GetLeaveApprovalListSpAsync(GetLeaveApprovalListRequest request);
    Task<IEnumerable<PendingApprovalByManagerDto>> GetPendingApprovalsByManagerSpAsync(int tenantId, int reportingManagerId, int pendingStatusId);
    Task<IEnumerable<PendingApprovalForHRDto>> GetPendingApprovalsForHRSpAsync(int tenantId, int pendingStatusId, int approvedStatusId);
}





