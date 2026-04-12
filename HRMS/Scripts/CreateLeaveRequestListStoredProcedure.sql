-- =============================================
-- Script: Create Leave Request List Stored Procedures
-- Description: Retrieves leave/OT request list for Leave Management module
-- =============================================

-- Drop existing procedures if they exist
IF OBJECT_ID('dbo.usp_GetLeaveRequestList', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetLeaveRequestList;
GO

IF OBJECT_ID('dbo.usp_GetLeaveRequestByStaff', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetLeaveRequestByStaff;
GO

-- =============================================
-- Stored Procedure: usp_GetLeaveRequestList
-- Description: Retrieves all leave/OT requests with filters
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetLeaveRequestList]
(
    @TenantId INT,
    @StaffId INT = NULL,
    @RequestTypeId INT = NULL,
    @LeaveTypeId INT = NULL,
    @LeaveStatus INT = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @SearchTerm NVARCHAR(200) = NULL,
    @IsActive BIT = 1,
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
      AND (@IsActive IS NULL OR lr.IsActive = @IsActive)
      AND (@StaffId IS NULL OR lr.StaffId = @StaffId)
      AND (@RequestTypeId IS NULL OR lr.RequestTypeId = @RequestTypeId)
      AND (@LeaveTypeId IS NULL OR lr.LeaveTypeId = @LeaveTypeId)
      AND (@LeaveStatus IS NULL OR lr.LeaveStatus = @LeaveStatus)
      AND (@FromDate IS NULL OR lr.FromDate >= @FromDate)
      AND (@ToDate IS NULL OR lr.ToDate <= @ToDate)
      AND (@SearchTerm IS NULL OR @SearchTerm = '' OR
           s.Name LIKE '%' + @SearchTerm + '%' OR
           s.EmployeeCode LIKE '%' + @SearchTerm + '%');

    -- Get paginated leave request list
    SELECT
        lr.RequestId,
        lr.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Division,
        s.Position,
        lr.RequestTypeId,
        rt.Name AS RequestTypeName,
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
        rm.EmployeeCode AS ReportingManagerCode,
        lr.HRApprovalRequired,
        lr.ApprovedBy_L1,
        l1.Name AS ApprovedByL1Name,
        lr.ApprovedDate_L1,
        lr.ApprovedBy_HR,
        hr.Name AS ApprovedByHRName,
        lr.ApprovedDate_HR,
        lr.Attachment,
        lr.IsActive,
        lr.TenantId,
        lr.CreatedDate,
        lr.CreatedBy,
        lr.UpdatedDate,
        lr.UpdatedBy
    FROM [dbo].[Leave_OT_Request] lr
    INNER JOIN [dbo].[Staff] s ON lr.StaffId = s.StaffId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] rt ON lr.RequestTypeId = rt.Id
    LEFT JOIN [dbo].[LeaveTypeMaster] lt ON lr.LeaveTypeId = lt.LeaveTypeId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] ls ON lr.LeaveStatus = ls.Id
    LEFT JOIN [dbo].[Staff] rm ON lr.ReportingManagerId = rm.StaffId
    LEFT JOIN [dbo].[Staff] l1 ON lr.ApprovedBy_L1 = l1.StaffId
    LEFT JOIN [dbo].[Staff] hr ON lr.ApprovedBy_HR = hr.StaffId
    WHERE lr.TenantId = @TenantId
      AND (@IsActive IS NULL OR lr.IsActive = @IsActive)
      AND (@StaffId IS NULL OR lr.StaffId = @StaffId)
      AND (@RequestTypeId IS NULL OR lr.RequestTypeId = @RequestTypeId)
      AND (@LeaveTypeId IS NULL OR lr.LeaveTypeId = @LeaveTypeId)
      AND (@LeaveStatus IS NULL OR lr.LeaveStatus = @LeaveStatus)
      AND (@FromDate IS NULL OR lr.FromDate >= @FromDate)
      AND (@ToDate IS NULL OR lr.ToDate <= @ToDate)
      AND (@SearchTerm IS NULL OR @SearchTerm = '' OR
           s.Name LIKE '%' + @SearchTerm + '%' OR
           s.EmployeeCode LIKE '%' + @SearchTerm + '%')
    ORDER BY lr.CreatedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- =============================================
-- Stored Procedure: usp_GetLeaveRequestByStaff
-- Description: Retrieves leave/OT requests for a specific staff member
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetLeaveRequestByStaff]
(
    @TenantId INT,
    @StaffId INT,
    @RequestTypeId INT = NULL,
    @Year INT = NULL,
    @IsActive BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        lr.RequestId,
        lr.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        lr.RequestTypeId,
        rt.Name AS RequestTypeName,
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
        lr.HRApprovalRequired,
        lr.ApprovedBy_L1,
        l1.Name AS ApprovedByL1Name,
        lr.ApprovedDate_L1,
        lr.ApprovedBy_HR,
        hr.Name AS ApprovedByHRName,
        lr.ApprovedDate_HR,
        lr.Attachment,
        lr.IsActive,
        lr.CreatedDate
    FROM [dbo].[Leave_OT_Request] lr
    INNER JOIN [dbo].[Staff] s ON lr.StaffId = s.StaffId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] rt ON lr.RequestTypeId = rt.Id
    LEFT JOIN [dbo].[LeaveTypeMaster] lt ON lr.LeaveTypeId = lt.LeaveTypeId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] ls ON lr.LeaveStatus = ls.Id
    LEFT JOIN [dbo].[Staff] rm ON lr.ReportingManagerId = rm.StaffId
    LEFT JOIN [dbo].[Staff] l1 ON lr.ApprovedBy_L1 = l1.StaffId
    LEFT JOIN [dbo].[Staff] hr ON lr.ApprovedBy_HR = hr.StaffId
    WHERE lr.TenantId = @TenantId
      AND lr.StaffId = @StaffId
      AND (@IsActive IS NULL OR lr.IsActive = @IsActive)
      AND (@RequestTypeId IS NULL OR lr.RequestTypeId = @RequestTypeId)
      AND (@Year IS NULL OR YEAR(lr.FromDate) = @Year)
    ORDER BY lr.CreatedDate DESC;
END;
GO

PRINT 'Leave Request stored procedures created successfully';
