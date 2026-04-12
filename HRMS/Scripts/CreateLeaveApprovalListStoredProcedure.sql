-- =============================================
-- Script: Create Leave Approval List Stored Procedures
-- Description: Retrieves leave/OT requests for approval workflow
-- =============================================

-- Drop existing procedures if they exist
IF OBJECT_ID('dbo.usp_GetLeaveApprovalList', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetLeaveApprovalList;
GO

IF OBJECT_ID('dbo.usp_GetPendingApprovalsByManager', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetPendingApprovalsByManager;
GO

IF OBJECT_ID('dbo.usp_GetPendingApprovalsForHR', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetPendingApprovalsForHR;
GO

-- =============================================
-- Stored Procedure: usp_GetLeaveApprovalList
-- Description: Retrieves all leave/OT requests for approval with comprehensive filters
-- Parameters: Supports filtering by manager, HR pending, status, date range, etc.
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetLeaveApprovalList]
(
    @TenantId INT,
    @ReportingManagerId INT = NULL,      -- Filter by reporting manager (for manager view)
    @ForHRApproval BIT = 0,              -- 1 = Show HR pending approvals only
    @PendingStatusId INT = NULL,         -- Status ID for PENDING
    @ApprovedStatusId INT = NULL,        -- Status ID for APPROVED (L1 approved, pending HR)
    @LeaveStatus INT = NULL,             -- Filter by specific status
    @RequestTypeId INT = NULL,           -- Filter by request type (Leave/OT)
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @ShowAllRequests BIT = 0,            -- 1 = Show all requests (HR view)
    @PageNumber INT = 1,
    @PageSize INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Get total count for pagination
    SELECT COUNT(*) AS TotalCount
    FROM [dbo].[Leave_OT_Request] lr
    INNER JOIN [dbo].[Staff] s ON lr.StaffId = s.StaffId
    WHERE lr.TenantId = @TenantId
      AND lr.IsActive = 1
      -- Manager filter
      AND (@ReportingManagerId IS NULL OR lr.ReportingManagerId = @ReportingManagerId)
      -- HR approval filter
      AND (@ForHRApproval = 0 OR
           (lr.HRApprovalRequired = 1
            AND lr.ApprovedBy_HR IS NULL
            AND (lr.LeaveStatus = @PendingStatusId OR lr.LeaveStatus = @ApprovedStatusId)))
      -- Status filter
      AND (@LeaveStatus IS NULL OR lr.LeaveStatus = @LeaveStatus)
      -- Request type filter
      AND (@RequestTypeId IS NULL OR lr.RequestTypeId = @RequestTypeId)
      -- Date range filter
      AND (@FromDate IS NULL OR lr.FromDate >= @FromDate)
      AND (@ToDate IS NULL OR lr.ToDate <= @ToDate)
      -- Search filter
      AND (@SearchTerm IS NULL OR @SearchTerm = '' OR
           s.Name LIKE '%' + @SearchTerm + '%' OR
           s.EmployeeCode LIKE '%' + @SearchTerm + '%');

    -- Get paginated approval list
    SELECT
        lr.RequestId,
        lr.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Division,
        s.Position,
        s.Photo AS StaffPhoto,
        lr.RequestTypeId,
        rt.Name AS RequestTypeName,
        rt.Code AS RequestTypeCode,
        lr.LeaveTypeId,
        lt.LeaveTypeName,
        lr.FromDate,
        lr.ToDate,
        lr.TotalDays,
        lr.TotalHours,
        lr.Reason,
        lr.LeaveStatus,
        ls.Name AS LeaveStatusName,
        ls.Code AS LeaveStatusCode,
        lr.ReportingManagerId,
        rm.Name AS ReportingManagerName,
        rm.EmployeeCode AS ReportingManagerCode,
        lr.HRApprovalRequired,
        lr.ApprovedBy_L1,
        l1.Name AS ApprovedByL1Name,
        lr.ApprovedDate_L1,
        lr.ApprovedBy_HR,
        hr.Name AS ApprovedByHRName,
        lr.ApprovedDate_HR,
        lr.Attachment,
        lr.CreatedDate,
        lr.CreatedBy,
        -- Calculated fields for approval workflow
        CASE
            WHEN lr.ApprovedBy_L1 IS NULL AND @PendingStatusId IS NOT NULL AND lr.LeaveStatus = @PendingStatusId THEN 'Pending L1'
            WHEN lr.ApprovedBy_L1 IS NOT NULL AND lr.HRApprovalRequired = 1 AND lr.ApprovedBy_HR IS NULL THEN 'Pending HR'
            WHEN lr.ApprovedBy_HR IS NOT NULL THEN 'Fully Approved'
            WHEN lr.ApprovedBy_L1 IS NOT NULL AND lr.HRApprovalRequired = 0 THEN 'Approved'
            ELSE 'Unknown'
        END AS ApprovalStage,
        -- Days until start
        DATEDIFF(DAY, GETDATE(), lr.FromDate) AS DaysUntilStart
    FROM [dbo].[Leave_OT_Request] lr
    INNER JOIN [dbo].[Staff] s ON lr.StaffId = s.StaffId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] rt ON lr.RequestTypeId = rt.Id
    LEFT JOIN [dbo].[LeaveTypeMaster] lt ON lr.LeaveTypeId = lt.LeaveTypeId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] ls ON lr.LeaveStatus = ls.Id
    LEFT JOIN [dbo].[Staff] rm ON lr.ReportingManagerId = rm.StaffId
    LEFT JOIN [dbo].[Staff] l1 ON lr.ApprovedBy_L1 = l1.StaffId
    LEFT JOIN [dbo].[Staff] hr ON lr.ApprovedBy_HR = hr.StaffId
    WHERE lr.TenantId = @TenantId
      AND lr.IsActive = 1
      -- Manager filter
      AND (@ReportingManagerId IS NULL OR lr.ReportingManagerId = @ReportingManagerId)
      -- HR approval filter
      AND (@ForHRApproval = 0 OR
           (lr.HRApprovalRequired = 1
            AND lr.ApprovedBy_HR IS NULL
            AND (lr.LeaveStatus = @PendingStatusId OR lr.LeaveStatus = @ApprovedStatusId)))
      -- Status filter
      AND (@LeaveStatus IS NULL OR lr.LeaveStatus = @LeaveStatus)
      -- Request type filter
      AND (@RequestTypeId IS NULL OR lr.RequestTypeId = @RequestTypeId)
      -- Date range filter
      AND (@FromDate IS NULL OR lr.FromDate >= @FromDate)
      AND (@ToDate IS NULL OR lr.ToDate <= @ToDate)
      -- Search filter
      AND (@SearchTerm IS NULL OR @SearchTerm = '' OR
           s.Name LIKE '%' + @SearchTerm + '%' OR
           s.EmployeeCode LIKE '%' + @SearchTerm + '%')
    ORDER BY lr.CreatedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- =============================================
-- Stored Procedure: usp_GetPendingApprovalsByManager
-- Description: Retrieves pending leave/OT requests for a specific manager (L1 approval)
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetPendingApprovalsByManager]
(
    @TenantId INT,
    @ReportingManagerId INT,
    @PendingStatusId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        lr.RequestId,
        lr.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Division,
        s.Position,
        s.Photo AS StaffPhoto,
        lr.RequestTypeId,
        rt.Name AS RequestTypeName,
        rt.Code AS RequestTypeCode,
        lr.LeaveTypeId,
        lt.LeaveTypeName,
        lr.FromDate,
        lr.ToDate,
        lr.TotalDays,
        lr.TotalHours,
        lr.Reason,
        lr.LeaveStatus,
        ls.Name AS LeaveStatusName,
        lr.HRApprovalRequired,
        lr.Attachment,
        lr.CreatedDate,
        DATEDIFF(DAY, GETDATE(), lr.FromDate) AS DaysUntilStart
    FROM [dbo].[Leave_OT_Request] lr
    INNER JOIN [dbo].[Staff] s ON lr.StaffId = s.StaffId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] rt ON lr.RequestTypeId = rt.Id
    LEFT JOIN [dbo].[LeaveTypeMaster] lt ON lr.LeaveTypeId = lt.LeaveTypeId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] ls ON lr.LeaveStatus = ls.Id
    WHERE lr.TenantId = @TenantId
      AND lr.IsActive = 1
      AND lr.ReportingManagerId = @ReportingManagerId
      AND lr.LeaveStatus = @PendingStatusId
    ORDER BY lr.CreatedDate DESC;
END;
GO

-- =============================================
-- Stored Procedure: usp_GetPendingApprovalsForHR
-- Description: Retrieves leave/OT requests pending HR approval
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetPendingApprovalsForHR]
(
    @TenantId INT,
    @PendingStatusId INT,
    @ApprovedStatusId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        lr.RequestId,
        lr.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Division,
        s.Position,
        s.Photo AS StaffPhoto,
        lr.RequestTypeId,
        rt.Name AS RequestTypeName,
        rt.Code AS RequestTypeCode,
        lr.LeaveTypeId,
        lt.LeaveTypeName,
        lr.FromDate,
        lr.ToDate,
        lr.TotalDays,
        lr.TotalHours,
        lr.Reason,
        lr.LeaveStatus,
        ls.Name AS LeaveStatusName,
        lr.ReportingManagerId,
        rm.Name AS ReportingManagerName,
        lr.ApprovedBy_L1,
        l1.Name AS ApprovedByL1Name,
        lr.ApprovedDate_L1,
        lr.Attachment,
        lr.CreatedDate,
        DATEDIFF(DAY, GETDATE(), lr.FromDate) AS DaysUntilStart,
        CASE
            WHEN lr.ApprovedBy_L1 IS NULL THEN 'Pending L1'
            ELSE 'Pending HR'
        END AS ApprovalStage
    FROM [dbo].[Leave_OT_Request] lr
    INNER JOIN [dbo].[Staff] s ON lr.StaffId = s.StaffId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] rt ON lr.RequestTypeId = rt.Id
    LEFT JOIN [dbo].[LeaveTypeMaster] lt ON lr.LeaveTypeId = lt.LeaveTypeId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] ls ON lr.LeaveStatus = ls.Id
    LEFT JOIN [dbo].[Staff] rm ON lr.ReportingManagerId = rm.StaffId
    LEFT JOIN [dbo].[Staff] l1 ON lr.ApprovedBy_L1 = l1.StaffId
    WHERE lr.TenantId = @TenantId
      AND lr.IsActive = 1
      AND lr.HRApprovalRequired = 1
      AND lr.ApprovedBy_HR IS NULL
      AND (lr.LeaveStatus = @PendingStatusId OR lr.LeaveStatus = @ApprovedStatusId)
    ORDER BY lr.CreatedDate DESC;
END;
GO

PRINT 'Leave Approval stored procedures created successfully';
