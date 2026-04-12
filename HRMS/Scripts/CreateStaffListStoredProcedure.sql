-- =============================================
-- Script: Create usp_GetStaffList Stored Procedure
-- Description: Retrieves staff list with filters for Employee Management module
-- =============================================

-- Drop existing procedure if it exists
IF OBJECT_ID('dbo.usp_GetStaffList', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetStaffList;
GO

CREATE PROCEDURE [dbo].[usp_GetStaffList]
(
    @TenantId INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @Division NVARCHAR(100) = NULL,
    @Department NVARCHAR(100) = NULL,
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
    FROM [dbo].[Staff] s
    WHERE s.TenantId = @TenantId
      AND (@IsActive IS NULL OR s.IsActive = @IsActive)
      AND (@Division IS NULL OR @Division = '' OR s.Division = @Division)
      AND (@Department IS NULL OR @Department = '' OR s.Department = @Department)
      AND (@SearchTerm IS NULL OR @SearchTerm = '' OR
           s.Name LIKE '%' + @SearchTerm + '%' OR
           s.EmployeeCode LIKE '%' + @SearchTerm + '%' OR
           s.Email LIKE '%' + @SearchTerm + '%' OR
           s.Division LIKE '%' + @SearchTerm + '%' OR
           s.Department LIKE '%' + @SearchTerm + '%');

    -- Get paginated staff list
    SELECT
        s.StaffId,
        s.EmployeeCode,
        s.Name,
        s.Email,
        s.PhoneNumber,
        s.Company,
        s.Division,
        s.Department,
        s.Position,
        s.DateOfBirth,
        s.DateJoined,
        s.RetirementDate,
        s.Photo,
        s.Address,
        s.IdentityCard,
        s.GenderId,
        g.Name AS GenderName,
        s.EmploymentStatusId,
        es.Name AS EmploymentStatusName,
        s.ReportingManagerId,
        rm.Name AS ReportingManagerName,
        rm.EmployeeCode AS ReportingManagerCode,
        s.IsActive,
        s.TenantId,
        s.CreatedDate,
        s.CreatedBy,
        s.UpdatedDate,
        s.UpdatedBy
    FROM [dbo].[Staff] s
    LEFT JOIN [dbo].[tbl_Master_Dropdown] g ON s.GenderId = g.Id AND g.Category = 'Gender'
    LEFT JOIN [dbo].[tbl_Master_Dropdown] es ON s.EmploymentStatusId = es.Id AND es.Category = 'EmploymentStatus'
    LEFT JOIN [dbo].[Staff] rm ON s.ReportingManagerId = rm.StaffId
    WHERE s.TenantId = @TenantId
      AND (@IsActive IS NULL OR s.IsActive = @IsActive)
      AND (@Division IS NULL OR @Division = '' OR s.Division = @Division)
      AND (@Department IS NULL OR @Department = '' OR s.Department = @Department)
      AND (@SearchTerm IS NULL OR @SearchTerm = '' OR
           s.Name LIKE '%' + @SearchTerm + '%' OR
           s.EmployeeCode LIKE '%' + @SearchTerm + '%' OR
           s.Email LIKE '%' + @SearchTerm + '%' OR
           s.Division LIKE '%' + @SearchTerm + '%' OR
           s.Department LIKE '%' + @SearchTerm + '%')
    ORDER BY s.Name ASC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

PRINT 'Stored procedure usp_GetStaffList created successfully';
