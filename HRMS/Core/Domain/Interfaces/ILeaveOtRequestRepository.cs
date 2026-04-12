using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;

namespace HRMS.Core.Domain.Interfaces;

public interface ILeaveOtRequestRepository : IRepository<LeaveOtRequest>
{
    Task<IEnumerable<LeaveOtRequest>> GetByStaffIdAsync(int staffId, int tenantId);
    Task<IEnumerable<LeaveOtRequest>> GetByReportingManagerIdAsync(int reportingManagerId, int tenantId);
    Task<IEnumerable<LeaveOtRequest>> GetPendingByReportingManagerIdAsync(int reportingManagerId, int tenantId, int? pendingStatusId = null);
    Task<IEnumerable<LeaveOtRequest>> GetPendingForHRAsync(int tenantId, int? pendingStatusId = null, int? approvedStatusId = null);
    Task<IEnumerable<LeaveOtRequest>> GetByTenantIdAsync(int tenantId, bool includeInactive = false);

    // Stored procedure methods for Leave Request List
    Task<(IEnumerable<LeaveRequestListSpDto> Items, int TotalCount)> GetLeaveRequestListSpAsync(GetLeaveRequestListRequest request);
    Task<IEnumerable<LeaveRequestListSpDto>> GetLeaveRequestByStaffSpAsync(int tenantId, int staffId, int? requestTypeId = null, int? year = null, bool isActive = true);

    // Stored procedure methods for Leave Approval
    Task<(IEnumerable<LeaveApprovalListSpDto> Items, int TotalCount)> GetLeaveApprovalListSpAsync(GetLeaveApprovalListRequest request);
    Task<IEnumerable<PendingApprovalByManagerDto>> GetPendingApprovalsByManagerSpAsync(int tenantId, int reportingManagerId, int pendingStatusId);
    Task<IEnumerable<PendingApprovalForHRDto>> GetPendingApprovalsForHRSpAsync(int tenantId, int pendingStatusId, int approvedStatusId);
}


