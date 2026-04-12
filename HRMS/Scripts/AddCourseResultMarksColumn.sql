-- =============================================
-- Script: Add Marks and CertificateSerialNumber to CourseResult Table
-- Description: Adds columns for marks tracking and certificate serial numbers
-- =============================================

-- Add Marks column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CourseResult]') AND name = 'Marks')
BEGIN
    ALTER TABLE [dbo].[CourseResult] ADD Marks DECIMAL(5,2) NULL;
    PRINT 'Column Marks added successfully';
END
ELSE
BEGIN
    PRINT 'Column Marks already exists';
END
GO

-- Add CertificateSerialNumber column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CourseResult]') AND name = 'CertificateSerialNumber')
BEGIN
    ALTER TABLE [dbo].[CourseResult] ADD CertificateSerialNumber NVARCHAR(50) NULL;
    PRINT 'Column CertificateSerialNumber added successfully';
END
ELSE
BEGIN
    PRINT 'Column CertificateSerialNumber already exists';
END
GO

-- Update the stored procedure for getting course results
IF OBJECT_ID('dbo.usp_GetCourseResultSummary', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCourseResultSummary;
GO

CREATE PROCEDURE [dbo].[usp_GetCourseResultSummary]
    @CoursePlanId INT,
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CR.ResultId,
        CR.CoursePlanId,
        CR.StaffId,
        S.Name AS StaffName,
        S.EmployeeCode,
        D.Name AS Department,
        P.Name AS Position,
        CR.TotalDays,
        CR.PresentDays,
        CR.AttendancePercentage,
        CR.Marks,
        CR.ResultStatus,
        CR.CertificatePath,
        CR.CertificateSerialNumber,
        CR.UpdatedDate
    FROM [dbo].[CourseResult] CR
    INNER JOIN [dbo].[Staff] S ON CR.StaffId = S.StaffId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] D ON S.DepartmentId = D.Id
    LEFT JOIN [dbo].[tbl_Master_Dropdown] P ON S.PositionId = P.Id
    WHERE CR.CoursePlanId = @CoursePlanId
        AND CR.TenantId = @TenantId
        AND CR.IsActive = 1
    ORDER BY S.Name;
END;
GO

-- Create procedure to update marks
IF OBJECT_ID('dbo.usp_UpdateCourseResultMarks', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_UpdateCourseResultMarks;
GO

CREATE PROCEDURE [dbo].[usp_UpdateCourseResultMarks]
    @ResultId INT,
    @TenantId INT,
    @Marks DECIMAL(5,2),
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[CourseResult]
    SET Marks = @Marks,
        UpdatedBy = @UpdatedBy,
        UpdatedDate = GETDATE()
    WHERE ResultId = @ResultId AND TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

-- Create function to generate certificate serial number
IF OBJECT_ID('dbo.fn_GenerateCertificateSerialNumber', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_GenerateCertificateSerialNumber;
GO

CREATE FUNCTION [dbo].[fn_GenerateCertificateSerialNumber]
(
    @CourseCode NVARCHAR(50),
    @TenantId INT,
    @ResultId INT
)
RETURNS NVARCHAR(50)
AS
BEGIN
    DECLARE @SerialNumber NVARCHAR(50);

    -- Format: CourseCode-TenantId-ResultId (e.g., CGM4-002-00070)
    SET @SerialNumber = @CourseCode + '-' +
                        RIGHT('000' + CAST(@TenantId AS NVARCHAR(3)), 3) + '-' +
                        RIGHT('00000' + CAST(@ResultId AS NVARCHAR(5)), 5);

    RETURN @SerialNumber;
END;
GO

PRINT 'Course Result columns and stored procedures created/updated successfully';
