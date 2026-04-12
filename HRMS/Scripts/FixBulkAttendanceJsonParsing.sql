-- =============================================
-- Script: Fix usp_BulkMarkCourseAttendanceDateWise JSON Boolean Parsing
-- Description: Updates the stored procedure to properly parse JSON boolean values
-- Run this script to fix the attendance checkbox not showing after save
-- =============================================

IF OBJECT_ID('dbo.usp_BulkMarkCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_BulkMarkCourseAttendanceDateWise;
GO

-- =============================================
-- Stored Procedure: usp_BulkMarkCourseAttendanceDateWise
-- Description: Marks attendance for multiple staff members for a specific date
-- Uses proper JSON boolean parsing
-- =============================================
CREATE PROCEDURE [dbo].[usp_BulkMarkCourseAttendanceDateWise]
(
    @CoursePlanId INT,
    @AttendanceDate DATE,
    @TenantId INT,
    @CreatedBy INT = NULL,
    @AttendanceData NVARCHAR(MAX) -- JSON array: [{"StaffId":1,"IsPresent":true,"Remarks":""},...]
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Parse JSON and process each attendance record
        DECLARE @StaffId INT, @IsPresent BIT, @Remarks NVARCHAR(500);

        DECLARE attendance_cursor CURSOR FOR
        SELECT
            CAST(JSON_VALUE(value, '$.StaffId') AS INT) AS StaffId,
            -- Properly convert JSON boolean to BIT
            CASE
                WHEN LOWER(JSON_VALUE(value, '$.IsPresent')) = 'true' THEN 1
                WHEN JSON_VALUE(value, '$.IsPresent') = '1' THEN 1
                ELSE 0
            END AS IsPresent,
            JSON_VALUE(value, '$.Remarks') AS Remarks
        FROM OPENJSON(@AttendanceData);

        OPEN attendance_cursor;
        FETCH NEXT FROM attendance_cursor INTO @StaffId, @IsPresent, @Remarks;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Check if record exists
            IF EXISTS (
                SELECT 1
                FROM [dbo].[CourseAttendance_DateWise]
                WHERE CoursePlanId = @CoursePlanId
                  AND StaffId = @StaffId
                  AND AttendanceDate = @AttendanceDate
                  AND TenantId = @TenantId
                  AND IsActive = 1
            )
            BEGIN
                -- Update existing
                UPDATE [dbo].[CourseAttendance_DateWise]
                SET IsPresent = @IsPresent,
                    Remarks = @Remarks,
                    UpdatedDate = GETDATE(),
                    UpdatedBy = @CreatedBy
                WHERE CoursePlanId = @CoursePlanId
                  AND StaffId = @StaffId
                  AND AttendanceDate = @AttendanceDate
                  AND TenantId = @TenantId
                  AND IsActive = 1;
            END
            ELSE
            BEGIN
                -- Insert new
                INSERT INTO [dbo].[CourseAttendance_DateWise] (
                    CoursePlanId, StaffId, AttendanceDate, IsPresent, Remarks,
                    TenantId, IsActive, CreatedDate, CreatedBy
                )
                VALUES (
                    @CoursePlanId, @StaffId, @AttendanceDate, @IsPresent, @Remarks,
                    @TenantId, 1, GETDATE(), @CreatedBy
                );
            END

            FETCH NEXT FROM attendance_cursor INTO @StaffId, @IsPresent, @Remarks;
        END

        CLOSE attendance_cursor;
        DEALLOCATE attendance_cursor;

        COMMIT TRANSACTION;

        SELECT 1 AS Success, 'Bulk attendance saved successfully' AS Message, CAST(NULL AS INT) AS AttendanceId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 0 AS Success, ERROR_MESSAGE() AS Message, CAST(NULL AS INT) AS AttendanceId;
    END CATCH
END;
GO

PRINT 'usp_BulkMarkCourseAttendanceDateWise updated successfully with proper JSON boolean parsing';
