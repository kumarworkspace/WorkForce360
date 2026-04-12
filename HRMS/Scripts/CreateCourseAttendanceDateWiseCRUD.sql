-- =============================================
-- Script: Create Course Attendance Date-wise CRUD Stored Procedures
-- Description: CRUD operations for date-wise attendance tracking
-- =============================================

-- Drop existing procedures if they exist
IF OBJECT_ID('dbo.usp_GetCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCourseAttendanceDateWise;
GO

IF OBJECT_ID('dbo.usp_CreateCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CreateCourseAttendanceDateWise;
GO

IF OBJECT_ID('dbo.usp_UpdateCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_UpdateCourseAttendanceDateWise;
GO

IF OBJECT_ID('dbo.usp_DeleteCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_DeleteCourseAttendanceDateWise;
GO

IF OBJECT_ID('dbo.usp_BulkMarkCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_BulkMarkCourseAttendanceDateWise;
GO

IF OBJECT_ID('dbo.usp_GetAttendanceSummaryByCoursePlan', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetAttendanceSummaryByCoursePlan;
GO

-- =============================================
-- Stored Procedure: usp_GetCourseAttendanceDateWise
-- Description: Retrieves date-wise attendance records for a course plan
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetCourseAttendanceDateWise]
(
    @CoursePlanId INT,
    @TenantId INT,
    @AttendanceDate DATE = NULL,
    @StaffId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ca.AttendanceId,
        ca.CoursePlanId,
        ca.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Division,
        s.Position,
        s.Photo AS StaffPhoto,
        ca.AttendanceDate,
        ca.IsPresent,
        ca.Remarks,
        ca.TenantId,
        ca.IsActive,
        ca.CreatedDate,
        ca.CreatedBy,
        ca.UpdatedDate,
        ca.UpdatedBy,
        -- Course details
        cp.StartDate AS CourseStartDate,
        cp.EndDate AS CourseEndDate,
        cr.Title AS CourseTitle,
        cr.CourseCode AS CourseNumber,
        trainer.Name AS TrainerName
    FROM [dbo].[CourseAttendance_DateWise] ca
    INNER JOIN [dbo].[Staff] s ON ca.StaffId = s.StaffId
    INNER JOIN [dbo].[CoursePlanning] cp ON ca.CoursePlanId = cp.Id
    INNER JOIN [dbo].[CourseRegistration] cr ON cp.CourseId = cr.CourseId
    INNER JOIN [dbo].[Staff] trainer ON cp.TrainerId = trainer.StaffId
    WHERE ca.CoursePlanId = @CoursePlanId
      AND ca.TenantId = @TenantId
      AND ca.IsActive = 1
      AND (@AttendanceDate IS NULL OR ca.AttendanceDate = @AttendanceDate)
      AND (@StaffId IS NULL OR ca.StaffId = @StaffId)
    ORDER BY ca.AttendanceDate DESC, s.Name ASC;
END;
GO

-- =============================================
-- Stored Procedure: usp_CreateCourseAttendanceDateWise
-- Description: Creates a new date-wise attendance record
-- =============================================
CREATE PROCEDURE [dbo].[usp_CreateCourseAttendanceDateWise]
(
    @CoursePlanId INT,
    @StaffId INT,
    @AttendanceDate DATE,
    @IsPresent BIT = 1,
    @Remarks NVARCHAR(500) = NULL,
    @TenantId INT,
    @CreatedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AttendanceId INT = -1;

    -- Check if attendance already exists for this staff on this date
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
        -- Update existing record instead
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

        SELECT @AttendanceId = AttendanceId
        FROM [dbo].[CourseAttendance_DateWise]
        WHERE CoursePlanId = @CoursePlanId
          AND StaffId = @StaffId
          AND AttendanceDate = @AttendanceDate
          AND TenantId = @TenantId
          AND IsActive = 1;

        SELECT @AttendanceId AS AttendanceId, 'Attendance updated successfully' AS Message, 1 AS Success;
        RETURN;
    END

    -- Insert new attendance record
    INSERT INTO [dbo].[CourseAttendance_DateWise] (
        CoursePlanId,
        StaffId,
        AttendanceDate,
        IsPresent,
        Remarks,
        TenantId,
        IsActive,
        CreatedDate,
        CreatedBy
    )
    VALUES (
        @CoursePlanId,
        @StaffId,
        @AttendanceDate,
        @IsPresent,
        @Remarks,
        @TenantId,
        1,
        GETDATE(),
        @CreatedBy
    );

    SET @AttendanceId = SCOPE_IDENTITY();

    SELECT @AttendanceId AS AttendanceId, 'Attendance created successfully' AS Message, 1 AS Success;
END;
GO

-- =============================================
-- Stored Procedure: usp_UpdateCourseAttendanceDateWise
-- Description: Updates an existing date-wise attendance record
-- =============================================
CREATE PROCEDURE [dbo].[usp_UpdateCourseAttendanceDateWise]
(
    @AttendanceId INT,
    @IsPresent BIT,
    @Remarks NVARCHAR(500) = NULL,
    @TenantId INT,
    @UpdatedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if record exists
    IF NOT EXISTS (
        SELECT 1
        FROM [dbo].[CourseAttendance_DateWise]
        WHERE AttendanceId = @AttendanceId
          AND TenantId = @TenantId
          AND IsActive = 1
    )
    BEGIN
        SELECT 0 AS Success, 'Attendance record not found' AS Message;
        RETURN;
    END

    -- Update the record
    UPDATE [dbo].[CourseAttendance_DateWise]
    SET IsPresent = @IsPresent,
        Remarks = @Remarks,
        UpdatedDate = GETDATE(),
        UpdatedBy = @UpdatedBy
    WHERE AttendanceId = @AttendanceId
      AND TenantId = @TenantId
      AND IsActive = 1;

    SELECT 1 AS Success, 'Attendance updated successfully' AS Message;
END;
GO

-- =============================================
-- Stored Procedure: usp_DeleteCourseAttendanceDateWise
-- Description: Soft deletes a date-wise attendance record
-- =============================================
CREATE PROCEDURE [dbo].[usp_DeleteCourseAttendanceDateWise]
(
    @AttendanceId INT,
    @TenantId INT,
    @UpdatedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if record exists
    IF NOT EXISTS (
        SELECT 1
        FROM [dbo].[CourseAttendance_DateWise]
        WHERE AttendanceId = @AttendanceId
          AND TenantId = @TenantId
          AND IsActive = 1
    )
    BEGIN
        SELECT 0 AS Success, 'Attendance record not found' AS Message;
        RETURN;
    END

    -- Soft delete the record
    UPDATE [dbo].[CourseAttendance_DateWise]
    SET IsActive = 0,
        UpdatedDate = GETDATE(),
        UpdatedBy = @UpdatedBy
    WHERE AttendanceId = @AttendanceId
      AND TenantId = @TenantId;

    SELECT 1 AS Success, 'Attendance deleted successfully' AS Message;
END;
GO

-- =============================================
-- Stored Procedure: usp_BulkMarkCourseAttendanceDateWise
-- Description: Marks attendance for multiple staff members for a specific date
-- Uses a temp table approach for bulk operations
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
            JSON_VALUE(value, '$.StaffId') AS StaffId,
            ISNULL(JSON_VALUE(value, '$.IsPresent'), 1) AS IsPresent,
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

        SELECT 1 AS Success, 'Bulk attendance saved successfully' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO

-- =============================================
-- Stored Procedure: usp_GetAttendanceSummaryByCoursePlan
-- Description: Gets attendance summary statistics for a course plan
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetAttendanceSummaryByCoursePlan]
(
    @CoursePlanId INT,
    @TenantId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Get course date range
    DECLARE @StartDate DATE, @EndDate DATE, @TotalDays INT;

    SELECT @StartDate = StartDate, @EndDate = EndDate
    FROM [dbo].[CoursePlanning]
    WHERE Id = @CoursePlanId AND TenantId = @TenantId;

    SET @TotalDays = DATEDIFF(DAY, @StartDate, @EndDate) + 1;

    -- Get attendance summary per staff
    SELECT
        s.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Division,
        s.Position,
        s.Photo AS StaffPhoto,
        @TotalDays AS TotalCourseDays,
        ISNULL(SUM(CASE WHEN ca.IsPresent = 1 THEN 1 ELSE 0 END), 0) AS DaysPresent,
        ISNULL(SUM(CASE WHEN ca.IsPresent = 0 THEN 1 ELSE 0 END), 0) AS DaysAbsent,
        ISNULL(COUNT(ca.AttendanceId), 0) AS DaysMarked,
        @TotalDays - ISNULL(COUNT(ca.AttendanceId), 0) AS DaysNotMarked,
        CASE
            WHEN @TotalDays > 0 THEN
                CAST(ISNULL(SUM(CASE WHEN ca.IsPresent = 1 THEN 1 ELSE 0 END), 0) * 100.0 / @TotalDays AS DECIMAL(5,2))
            ELSE 0
        END AS AttendancePercentage
    FROM [dbo].[CourseParticipant] cp
    INNER JOIN [dbo].[Staff] s ON cp.StaffId = s.StaffId
    LEFT JOIN [dbo].[CourseAttendance_DateWise] ca ON
        ca.CoursePlanId = cp.CoursePlanId AND
        ca.StaffId = cp.StaffId AND
        ca.TenantId = @TenantId AND
        ca.IsActive = 1
    WHERE cp.CoursePlanId = @CoursePlanId
      AND cp.TenantId = @TenantId
      AND cp.IsActive = 1
    GROUP BY s.StaffId, s.Name, s.EmployeeCode, s.Department, s.Division, s.Position, s.Photo
    ORDER BY s.Name;

    -- Get daily attendance summary
    SELECT
        ca.AttendanceDate,
        COUNT(*) AS TotalParticipants,
        SUM(CASE WHEN ca.IsPresent = 1 THEN 1 ELSE 0 END) AS PresentCount,
        SUM(CASE WHEN ca.IsPresent = 0 THEN 1 ELSE 0 END) AS AbsentCount
    FROM [dbo].[CourseAttendance_DateWise] ca
    WHERE ca.CoursePlanId = @CoursePlanId
      AND ca.TenantId = @TenantId
      AND ca.IsActive = 1
    GROUP BY ca.AttendanceDate
    ORDER BY ca.AttendanceDate;
END;
GO

PRINT 'Course Attendance Date-wise CRUD stored procedures created successfully';
