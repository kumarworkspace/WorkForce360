using Microsoft.EntityFrameworkCore;
using Npgsql;
using HRMS.Core.Application.DTOs;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories;

public class LeaveOtRequestRepository : Repository<LeaveOtRequest>, ILeaveOtRequestRepository
{
    public LeaveOtRequestRepository(HRMSDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveOtRequest>> GetByStaffIdAsync(int staffId, int tenantId)
    {
        return await _dbSet
            .Where(r => r.StaffId == staffId && r.TenantId == tenantId && r.IsActive)
            .Include(r => r.LeaveType)
            .Include(r => r.Staff)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveOtRequest>> GetByReportingManagerIdAsync(int reportingManagerId, int tenantId)
    {
        return await _dbSet
            .Where(r => r.ReportingManagerId == reportingManagerId && r.TenantId == tenantId && r.IsActive)
            .Include(r => r.LeaveType)
            .Include(r => r.Staff)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveOtRequest>> GetPendingByReportingManagerIdAsync(int reportingManagerId, int tenantId, int? pendingStatusId = null)
    {
        // Use provided status ID or default to 1 for backward compatibility
        var statusId = pendingStatusId ?? 1;
        
        return await _dbSet
            .Where(r => r.ReportingManagerId == reportingManagerId && 
                       r.TenantId == tenantId && 
                       r.IsActive &&
                       r.LeaveStatus == statusId) // PENDING
            .Include(r => r.LeaveType)
            .Include(r => r.Staff)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveOtRequest>> GetPendingForHRAsync(int tenantId, int? pendingStatusId = null, int? approvedStatusId = null)
    {
        // Use provided status IDs or default to 1 and 2 for backward compatibility
        var pendingId = pendingStatusId ?? 1;
        var approvedId = approvedStatusId ?? 2;
        
        // Get requests that need HR approval (HRApprovalRequired = true) and are pending or approved by L1
        var query = _dbSet
            .Where(r => r.TenantId == tenantId && 
                       r.IsActive &&
                       r.HRApprovalRequired &&
                       (r.LeaveStatus == pendingId || r.LeaveStatus == approvedId) && // PENDING or APPROVED by L1
                       r.ApprovedBy_HR == null) // Not yet approved by HR
            .Include(r => r.LeaveType)
            .Include(r => r.Staff)
            .Include(r => r.ReportingManager);
        
        return await query
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeaveOtRequest>> GetByTenantIdAsync(int tenantId, bool includeInactive = false)
    {
        var query = _dbSet
            .Where(r => r.TenantId == tenantId)
            .Include(r => r.LeaveType)
            .Include(r => r.Staff)
            .Include(r => r.ReportingManager)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        return await query.OrderByDescending(r => r.CreatedDate).ToListAsync();
    }

    #region Stored Procedure Methods

    public async Task<(IEnumerable<LeaveRequestListSpDto> Items, int TotalCount)> GetLeaveRequestListSpAsync(GetLeaveRequestListRequest request)
    {
        var parameters = new[]
        {
            new NpgsqlParameter("@TenantId", request.TenantId),
            new NpgsqlParameter("@StaffId", (object?)request.StaffId ?? DBNull.Value),
            new NpgsqlParameter("@RequestTypeId", (object?)request.RequestTypeId ?? DBNull.Value),
            new NpgsqlParameter("@LeaveTypeId", (object?)request.LeaveTypeId ?? DBNull.Value),
            new NpgsqlParameter("@LeaveStatus", (object?)request.LeaveStatus ?? DBNull.Value),
            new NpgsqlParameter("@FromDate", (object?)request.FromDate ?? DBNull.Value),
            new NpgsqlParameter("@ToDate", (object?)request.ToDate ?? DBNull.Value),
            new NpgsqlParameter("@SearchTerm", (object?)request.SearchTerm ?? DBNull.Value),
            new NpgsqlParameter("@IsActive", (object?)request.IsActive ?? DBNull.Value),
            new NpgsqlParameter("@PageNumber", request.PageNumber),
            new NpgsqlParameter("@PageSize", request.PageSize)
        };

        // PostgreSQL function returns single result set with TotalCount as a column (COUNT(*) OVER())
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT * FROM usp_GetLeaveRequestList(@TenantId, @StaffId, @RequestTypeId, @LeaveTypeId, @LeaveStatus, @FromDate, @ToDate, @SearchTerm, @IsActive, @PageNumber, @PageSize)";
        command.CommandType = System.Data.CommandType.Text;
        command.Parameters.AddRange(parameters);

        await _context.Database.OpenConnectionAsync();
        try
        {
            using var reader = await command.ExecuteReaderAsync();

            int totalCount = 0;
            var items = new List<LeaveRequestListSpDto>();
            while (await reader.ReadAsync())
            {
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                items.Add(MapLeaveRequestListSpDto(reader));
            }

            return (items, totalCount);
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<IEnumerable<LeaveRequestListSpDto>> GetLeaveRequestByStaffSpAsync(int tenantId, int staffId, int? requestTypeId = null, int? year = null, bool isActive = true)
    {
        var parameters = new[]
        {
            new NpgsqlParameter("@TenantId", tenantId),
            new NpgsqlParameter("@StaffId", staffId),
            new NpgsqlParameter("@RequestTypeId", (object?)requestTypeId ?? DBNull.Value),
            new NpgsqlParameter("@Year", (object?)year ?? DBNull.Value),
            new NpgsqlParameter("@IsActive", isActive)
        };

        var result = await _context.Set<LeaveRequestListSpDto>()
            .FromSqlRaw("SELECT * FROM usp_GetLeaveRequestByStaff(@TenantId, @StaffId, @RequestTypeId, @Year, @IsActive)", parameters)
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

    public async Task<(IEnumerable<LeaveApprovalListSpDto> Items, int TotalCount)> GetLeaveApprovalListSpAsync(GetLeaveApprovalListRequest request)
    {
        var parameters = new[]
        {
            new NpgsqlParameter("@TenantId", request.TenantId),
            new NpgsqlParameter("@ReportingManagerId", (object?)request.ReportingManagerId ?? DBNull.Value),
            new NpgsqlParameter("@ForHRApproval", request.ForHRApproval),
            new NpgsqlParameter("@PendingStatusId", (object?)request.PendingStatusId ?? DBNull.Value),
            new NpgsqlParameter("@ApprovedStatusId", (object?)request.ApprovedStatusId ?? DBNull.Value),
            new NpgsqlParameter("@LeaveStatus", (object?)request.LeaveStatus ?? DBNull.Value),
            new NpgsqlParameter("@RequestTypeId", (object?)request.RequestTypeId ?? DBNull.Value),
            new NpgsqlParameter("@FromDate", (object?)request.FromDate ?? DBNull.Value),
            new NpgsqlParameter("@ToDate", (object?)request.ToDate ?? DBNull.Value),
            new NpgsqlParameter("@SearchTerm", (object?)request.SearchTerm ?? DBNull.Value),
            new NpgsqlParameter("@ShowAllRequests", request.ShowAllRequests),
            new NpgsqlParameter("@PageNumber", request.PageNumber),
            new NpgsqlParameter("@PageSize", request.PageSize)
        };

        // PostgreSQL function returns single result set with TotalCount as a column (COUNT(*) OVER())
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT * FROM usp_GetLeaveApprovalList(@TenantId, @ReportingManagerId, @ForHRApproval, @PendingStatusId, @ApprovedStatusId, @LeaveStatus, @RequestTypeId, @FromDate, @ToDate, @SearchTerm, @ShowAllRequests, @PageNumber, @PageSize)";
        command.CommandType = System.Data.CommandType.Text;
        command.Parameters.AddRange(parameters);

        await _context.Database.OpenConnectionAsync();
        try
        {
            using var reader = await command.ExecuteReaderAsync();

            int totalCount = 0;
            var items = new List<LeaveApprovalListSpDto>();
            while (await reader.ReadAsync())
            {
                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                items.Add(MapLeaveApprovalListSpDto(reader));
            }

            return (items, totalCount);
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<IEnumerable<PendingApprovalByManagerDto>> GetPendingApprovalsByManagerSpAsync(int tenantId, int reportingManagerId, int pendingStatusId)
    {
        var parameters = new[]
        {
            new NpgsqlParameter("@TenantId", tenantId),
            new NpgsqlParameter("@ReportingManagerId", reportingManagerId),
            new NpgsqlParameter("@PendingStatusId", pendingStatusId)
        };

        var result = await _context.Set<PendingApprovalByManagerDto>()
            .FromSqlRaw("SELECT * FROM usp_GetPendingApprovalsByManager(@TenantId, @ReportingManagerId, @PendingStatusId)", parameters)
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

    public async Task<IEnumerable<PendingApprovalForHRDto>> GetPendingApprovalsForHRSpAsync(int tenantId, int pendingStatusId, int approvedStatusId)
    {
        var parameters = new[]
        {
            new NpgsqlParameter("@TenantId", tenantId),
            new NpgsqlParameter("@PendingStatusId", pendingStatusId),
            new NpgsqlParameter("@ApprovedStatusId", approvedStatusId)
        };

        var result = await _context.Set<PendingApprovalForHRDto>()
            .FromSqlRaw("SELECT * FROM usp_GetPendingApprovalsForHR(@TenantId, @PendingStatusId, @ApprovedStatusId)", parameters)
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

    #endregion

    #region Private Helper Methods

    private static LeaveRequestListSpDto MapLeaveRequestListSpDto(System.Data.Common.DbDataReader reader)
    {
        return new LeaveRequestListSpDto
        {
            RequestId = reader.GetInt32(reader.GetOrdinal("RequestId")),
            StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
            StaffName = reader.IsDBNull(reader.GetOrdinal("StaffName")) ? string.Empty : reader.GetString(reader.GetOrdinal("StaffName")),
            EmployeeCode = reader.IsDBNull(reader.GetOrdinal("EmployeeCode")) ? null : reader.GetString(reader.GetOrdinal("EmployeeCode")),
            Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? null : reader.GetString(reader.GetOrdinal("Department")),
            Division = reader.IsDBNull(reader.GetOrdinal("Division")) ? null : reader.GetString(reader.GetOrdinal("Division")),
            Position = reader.IsDBNull(reader.GetOrdinal("Position")) ? null : reader.GetString(reader.GetOrdinal("Position")),
            RequestTypeId = reader.GetInt32(reader.GetOrdinal("RequestTypeId")),
            RequestTypeName = reader.IsDBNull(reader.GetOrdinal("RequestTypeName")) ? null : reader.GetString(reader.GetOrdinal("RequestTypeName")),
            LeaveTypeId = reader.IsDBNull(reader.GetOrdinal("LeaveTypeId")) ? null : reader.GetInt32(reader.GetOrdinal("LeaveTypeId")),
            LeaveTypeName = reader.IsDBNull(reader.GetOrdinal("LeaveTypeName")) ? null : reader.GetString(reader.GetOrdinal("LeaveTypeName")),
            FromDate = reader.GetDateTime(reader.GetOrdinal("FromDate")),
            ToDate = reader.GetDateTime(reader.GetOrdinal("ToDate")),
            TotalDays = reader.IsDBNull(reader.GetOrdinal("TotalDays")) ? null : reader.GetDecimal(reader.GetOrdinal("TotalDays")),
            TotalHours = reader.IsDBNull(reader.GetOrdinal("TotalHours")) ? null : reader.GetDecimal(reader.GetOrdinal("TotalHours")),
            Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
            LeaveStatus = reader.IsDBNull(reader.GetOrdinal("LeaveStatus")) ? null : reader.GetInt32(reader.GetOrdinal("LeaveStatus")),
            LeaveStatusName = reader.IsDBNull(reader.GetOrdinal("LeaveStatusName")) ? null : reader.GetString(reader.GetOrdinal("LeaveStatusName")),
            ReportingManagerId = reader.IsDBNull(reader.GetOrdinal("ReportingManagerId")) ? null : reader.GetInt32(reader.GetOrdinal("ReportingManagerId")),
            ReportingManagerName = reader.IsDBNull(reader.GetOrdinal("ReportingManagerName")) ? null : reader.GetString(reader.GetOrdinal("ReportingManagerName")),
            ReportingManagerCode = reader.IsDBNull(reader.GetOrdinal("ReportingManagerCode")) ? null : reader.GetString(reader.GetOrdinal("ReportingManagerCode")),
            HRApprovalRequired = reader.GetBoolean(reader.GetOrdinal("HRApprovalRequired")),
            ApprovedBy_L1 = reader.IsDBNull(reader.GetOrdinal("ApprovedBy_L1")) ? null : reader.GetInt32(reader.GetOrdinal("ApprovedBy_L1")),
            ApprovedByL1Name = reader.IsDBNull(reader.GetOrdinal("ApprovedByL1Name")) ? null : reader.GetString(reader.GetOrdinal("ApprovedByL1Name")),
            ApprovedDate_L1 = reader.IsDBNull(reader.GetOrdinal("ApprovedDate_L1")) ? null : reader.GetDateTime(reader.GetOrdinal("ApprovedDate_L1")),
            ApprovedBy_HR = reader.IsDBNull(reader.GetOrdinal("ApprovedBy_HR")) ? null : reader.GetInt32(reader.GetOrdinal("ApprovedBy_HR")),
            ApprovedByHRName = reader.IsDBNull(reader.GetOrdinal("ApprovedByHRName")) ? null : reader.GetString(reader.GetOrdinal("ApprovedByHRName")),
            ApprovedDate_HR = reader.IsDBNull(reader.GetOrdinal("ApprovedDate_HR")) ? null : reader.GetDateTime(reader.GetOrdinal("ApprovedDate_HR")),
            Attachment = reader.IsDBNull(reader.GetOrdinal("Attachment")) ? null : reader.GetString(reader.GetOrdinal("Attachment")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            TenantId = reader.GetInt32(reader.GetOrdinal("TenantId")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
            UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
            UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedBy"))
        };
    }

    private static LeaveApprovalListSpDto MapLeaveApprovalListSpDto(System.Data.Common.DbDataReader reader)
    {
        return new LeaveApprovalListSpDto
        {
            RequestId = reader.GetInt32(reader.GetOrdinal("RequestId")),
            StaffId = reader.GetInt32(reader.GetOrdinal("StaffId")),
            StaffName = reader.IsDBNull(reader.GetOrdinal("StaffName")) ? string.Empty : reader.GetString(reader.GetOrdinal("StaffName")),
            EmployeeCode = reader.IsDBNull(reader.GetOrdinal("EmployeeCode")) ? null : reader.GetString(reader.GetOrdinal("EmployeeCode")),
            Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? null : reader.GetString(reader.GetOrdinal("Department")),
            Division = reader.IsDBNull(reader.GetOrdinal("Division")) ? null : reader.GetString(reader.GetOrdinal("Division")),
            Position = reader.IsDBNull(reader.GetOrdinal("Position")) ? null : reader.GetString(reader.GetOrdinal("Position")),
            StaffPhoto = reader.IsDBNull(reader.GetOrdinal("StaffPhoto")) ? null : reader.GetString(reader.GetOrdinal("StaffPhoto")),
            RequestTypeId = reader.GetInt32(reader.GetOrdinal("RequestTypeId")),
            RequestTypeName = reader.IsDBNull(reader.GetOrdinal("RequestTypeName")) ? null : reader.GetString(reader.GetOrdinal("RequestTypeName")),
            RequestTypeCode = reader.IsDBNull(reader.GetOrdinal("RequestTypeCode")) ? null : reader.GetString(reader.GetOrdinal("RequestTypeCode")),
            LeaveTypeId = reader.IsDBNull(reader.GetOrdinal("LeaveTypeId")) ? null : reader.GetInt32(reader.GetOrdinal("LeaveTypeId")),
            LeaveTypeName = reader.IsDBNull(reader.GetOrdinal("LeaveTypeName")) ? null : reader.GetString(reader.GetOrdinal("LeaveTypeName")),
            FromDate = reader.GetDateTime(reader.GetOrdinal("FromDate")),
            ToDate = reader.GetDateTime(reader.GetOrdinal("ToDate")),
            TotalDays = reader.IsDBNull(reader.GetOrdinal("TotalDays")) ? null : reader.GetDecimal(reader.GetOrdinal("TotalDays")),
            TotalHours = reader.IsDBNull(reader.GetOrdinal("TotalHours")) ? null : reader.GetDecimal(reader.GetOrdinal("TotalHours")),
            Reason = reader.IsDBNull(reader.GetOrdinal("Reason")) ? null : reader.GetString(reader.GetOrdinal("Reason")),
            LeaveStatus = reader.IsDBNull(reader.GetOrdinal("LeaveStatus")) ? null : reader.GetInt32(reader.GetOrdinal("LeaveStatus")),
            LeaveStatusName = reader.IsDBNull(reader.GetOrdinal("LeaveStatusName")) ? null : reader.GetString(reader.GetOrdinal("LeaveStatusName")),
            LeaveStatusCode = reader.IsDBNull(reader.GetOrdinal("LeaveStatusCode")) ? null : reader.GetString(reader.GetOrdinal("LeaveStatusCode")),
            ReportingManagerId = reader.IsDBNull(reader.GetOrdinal("ReportingManagerId")) ? null : reader.GetInt32(reader.GetOrdinal("ReportingManagerId")),
            ReportingManagerName = reader.IsDBNull(reader.GetOrdinal("ReportingManagerName")) ? null : reader.GetString(reader.GetOrdinal("ReportingManagerName")),
            ReportingManagerCode = reader.IsDBNull(reader.GetOrdinal("ReportingManagerCode")) ? null : reader.GetString(reader.GetOrdinal("ReportingManagerCode")),
            HRApprovalRequired = reader.GetBoolean(reader.GetOrdinal("HRApprovalRequired")),
            ApprovedBy_L1 = reader.IsDBNull(reader.GetOrdinal("ApprovedBy_L1")) ? null : reader.GetInt32(reader.GetOrdinal("ApprovedBy_L1")),
            ApprovedByL1Name = reader.IsDBNull(reader.GetOrdinal("ApprovedByL1Name")) ? null : reader.GetString(reader.GetOrdinal("ApprovedByL1Name")),
            ApprovedDate_L1 = reader.IsDBNull(reader.GetOrdinal("ApprovedDate_L1")) ? null : reader.GetDateTime(reader.GetOrdinal("ApprovedDate_L1")),
            ApprovedBy_HR = reader.IsDBNull(reader.GetOrdinal("ApprovedBy_HR")) ? null : reader.GetInt32(reader.GetOrdinal("ApprovedBy_HR")),
            ApprovedByHRName = reader.IsDBNull(reader.GetOrdinal("ApprovedByHRName")) ? null : reader.GetString(reader.GetOrdinal("ApprovedByHRName")),
            ApprovedDate_HR = reader.IsDBNull(reader.GetOrdinal("ApprovedDate_HR")) ? null : reader.GetDateTime(reader.GetOrdinal("ApprovedDate_HR")),
            Attachment = reader.IsDBNull(reader.GetOrdinal("Attachment")) ? null : reader.GetString(reader.GetOrdinal("Attachment")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
            ApprovalStage = reader.IsDBNull(reader.GetOrdinal("ApprovalStage")) ? null : reader.GetString(reader.GetOrdinal("ApprovalStage")),
            DaysUntilStart = reader.GetInt32(reader.GetOrdinal("DaysUntilStart"))
        };
    }

    #endregion
}

