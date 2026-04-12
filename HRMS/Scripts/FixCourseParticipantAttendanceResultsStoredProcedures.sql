-- =============================================
-- Script: Fix Course Participant, Attendance, and Results Stored Procedures
-- Description: Creates/updates stored procedures with correct table names
-- Run this script to fix the Attendance and Results tabs not showing data
-- =============================================

-- =============================================
-- First, check and rename table if needed (CourseAttendance_DateWise is the correct name)
-- =============================================

-- If the old table name exists, rename it
IF OBJECT_ID('dbo.Course_Attendance_DateWise', 'U') IS NOT NULL
    AND OBJECT_ID('dbo.CourseAttendance_DateWise', 'U') IS NULL
BEGIN
    EXEC sp_rename 'dbo.Course_Attendance_DateWise', 'CourseAttendance_DateWise';
    PRINT 'Table Course_Attendance_DateWise renamed to CourseAttendance_DateWise';
END
GO

-- =============================================
-- Create CourseAttendance_DateWise table if it doesn't exist
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseAttendance_DateWise')
BEGIN
    CREATE TABLE [dbo].[CourseAttendance_DateWise] (
        AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
        CoursePlanId INT NOT NULL,
        StaffId INT NOT NULL,
        AttendanceDate DATE NOT NULL,
        IsPresent BIT NOT NULL DEFAULT 1,
        Remarks NVARCHAR(500) NULL,
        TenantId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        CreatedBy INT NULL,
        UpdatedDate DATETIME2(7) NULL,
        UpdatedBy INT NULL,

        CONSTRAINT FK_CourseAttendanceDateWise_CoursePlanning FOREIGN KEY (CoursePlanId)
            REFERENCES [dbo].[CoursePlanning](Id),
        CONSTRAINT FK_CourseAttendanceDateWise_Staff FOREIGN KEY (StaffId)
            REFERENCES [dbo].[Staff](StaffId)
    );

    CREATE NONCLUSTERED INDEX IX_CourseAttendanceDateWise_TenantId
        ON [dbo].[CourseAttendance_DateWise](TenantId);
    CREATE NONCLUSTERED INDEX IX_CourseAttendanceDateWise_CoursePlanId
        ON [dbo].[CourseAttendance_DateWise](CoursePlanId);
    CREATE NONCLUSTERED INDEX IX_CourseAttendanceDateWise_StaffId
        ON [dbo].[CourseAttendance_DateWise](StaffId);
    CREATE NONCLUSTERED INDEX IX_CourseAttendanceDateWise_AttendanceDate
        ON [dbo].[CourseAttendance_DateWise](AttendanceDate);

    PRINT 'Table CourseAttendance_DateWise created successfully';
END
GO

-- =============================================
-- Create CourseParticipant table if it doesn't exist
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseParticipant')
BEGIN
    CREATE TABLE [dbo].[CourseParticipant] (
        CourseParticipantId INT IDENTITY(1,1) PRIMARY KEY,
        CoursePlanId INT NOT NULL,
        StaffId INT NOT NULL,
        TenantId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        CreatedBy INT NULL,
        UpdatedDate DATETIME2(7) NULL,
        UpdatedBy INT NULL,

        CONSTRAINT FK_CourseParticipant_CoursePlanning FOREIGN KEY (CoursePlanId)
            REFERENCES [dbo].[CoursePlanning](Id),
        CONSTRAINT FK_CourseParticipant_Staff FOREIGN KEY (StaffId)
            REFERENCES [dbo].[Staff](StaffId)
    );

    CREATE NONCLUSTERED INDEX IX_CourseParticipant_TenantId
        ON [dbo].[CourseParticipant](TenantId);
    CREATE NONCLUSTERED INDEX IX_CourseParticipant_CoursePlanId
        ON [dbo].[CourseParticipant](CoursePlanId);
    CREATE NONCLUSTERED INDEX IX_CourseParticipant_StaffId
        ON [dbo].[CourseParticipant](StaffId);

    PRINT 'Table CourseParticipant created successfully';
END
GO

-- =============================================
-- Create CourseResult table if it doesn't exist
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseResult')
BEGIN
    CREATE TABLE [dbo].[CourseResult] (
        ResultId INT IDENTITY(1,1) PRIMARY KEY,
        CoursePlanId INT NOT NULL,
        StaffId INT NOT NULL,
        TotalDays INT NOT NULL DEFAULT 0,
        PresentDays INT NOT NULL DEFAULT 0,
        AttendancePercentage DECIMAL(5,2) NOT NULL DEFAULT 0,
        Marks DECIMAL(5,2) NULL,
        ResultStatus NVARCHAR(10) NULL,
        CertificatePath NVARCHAR(500) NULL,
        CertificateSerialNumber NVARCHAR(50) NULL,
        TenantId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        CreatedBy INT NULL,
        UpdatedDate DATETIME2(7) NULL,
        UpdatedBy INT NULL,

        CONSTRAINT FK_CourseResult_CoursePlanning FOREIGN KEY (CoursePlanId)
            REFERENCES [dbo].[CoursePlanning](Id),
        CONSTRAINT FK_CourseResult_Staff FOREIGN KEY (StaffId)
            REFERENCES [dbo].[Staff](StaffId)
    );

    CREATE NONCLUSTERED INDEX IX_CourseResult_TenantId
        ON [dbo].[CourseResult](TenantId);
    CREATE NONCLUSTERED INDEX IX_CourseResult_CoursePlanId
        ON [dbo].[CourseResult](CoursePlanId);
    CREATE NONCLUSTERED INDEX IX_CourseResult_StaffId
        ON [dbo].[CourseResult](StaffId);

    PRINT 'Table CourseResult created successfully';
END
GO

-- Add Marks column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CourseResult]') AND name = 'Marks')
BEGIN
    ALTER TABLE [dbo].[CourseResult] ADD Marks DECIMAL(5,2) NULL;
    PRINT 'Column Marks added to CourseResult';
END
GO

-- Add CertificateSerialNumber column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CourseResult]') AND name = 'CertificateSerialNumber')
BEGIN
    ALTER TABLE [dbo].[CourseResult] ADD CertificateSerialNumber NVARCHAR(50) NULL;
    PRINT 'Column CertificateSerialNumber added to CourseResult';
END
GO

-- =============================================
-- Stored Procedure: usp_GetCourseParticipants
-- Description: Gets course participants with staff details
-- =============================================
IF OBJECT_ID('dbo.usp_GetCourseParticipants', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCourseParticipants;
GO

CREATE PROCEDURE [dbo].[usp_GetCourseParticipants]
    @CoursePlanId INT,
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cp.CourseParticipantId,
        cp.CoursePlanId,
        cp.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Position,
        s.Email,
        s.PhoneNumber,
        cp.CreatedDate
    FROM [dbo].[CourseParticipant] cp
    INNER JOIN [dbo].[Staff] s ON cp.StaffId = s.StaffId
    WHERE cp.CoursePlanId = @CoursePlanId
        AND cp.TenantId = @TenantId
        AND cp.IsActive = 1
        AND s.IsActive = 1
    ORDER BY s.Name ASC;
END;
GO

PRINT 'Stored procedure usp_GetCourseParticipants created/updated';
GO

-- =============================================
-- Stored Procedure: usp_GetCourseAttendanceDateWise
-- Description: Gets attendance records by course plan (uses correct table name)
-- =============================================
IF OBJECT_ID('dbo.usp_GetCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCourseAttendanceDateWise;
GO

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

PRINT 'Stored procedure usp_GetCourseAttendanceDateWise created/updated';
GO

-- =============================================
-- Stored Procedure: usp_GetCourseResultSummary
-- Description: Gets course result summary (includes Marks and CertificateSerialNumber)
-- Note: This returns participants WITH their results. If no result exists, creates one.
-- =============================================
IF OBJECT_ID('dbo.usp_GetCourseResultSummary', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCourseResultSummary;
GO

CREATE PROCEDURE [dbo].[usp_GetCourseResultSummary]
    @CoursePlanId INT,
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Get course date range for calculating total days
    DECLARE @StartDate DATE, @EndDate DATE, @TotalDays INT;

    SELECT @StartDate = StartDate, @EndDate = EndDate
    FROM [dbo].[CoursePlanning]
    WHERE Id = @CoursePlanId AND TenantId = @TenantId;

    SET @TotalDays = DATEDIFF(DAY, @StartDate, @EndDate) + 1;

    -- Return participants with their results (LEFT JOIN to include participants without results)
    SELECT
        ISNULL(CR.ResultId, 0) AS ResultId,
        CP.CoursePlanId,
        CP.StaffId,
        S.Name AS StaffName,
        S.EmployeeCode,
        S.Department,
        S.Position,
        ISNULL(CR.TotalDays, @TotalDays) AS TotalDays,
        ISNULL(CR.PresentDays, 0) AS PresentDays,
        ISNULL(CR.AttendancePercentage, 0) AS AttendancePercentage,
        CR.Marks,
        CR.ResultStatus,
        CR.CertificatePath,
        CR.CertificateSerialNumber,
        CR.UpdatedDate
    FROM [dbo].[CourseParticipant] CP
    INNER JOIN [dbo].[Staff] S ON CP.StaffId = S.StaffId
    LEFT JOIN [dbo].[CourseResult] CR ON
        CR.CoursePlanId = CP.CoursePlanId
        AND CR.StaffId = CP.StaffId
        AND CR.TenantId = @TenantId
        AND CR.IsActive = 1
    WHERE CP.CoursePlanId = @CoursePlanId
        AND CP.TenantId = @TenantId
        AND CP.IsActive = 1
        AND S.IsActive = 1
    ORDER BY S.Name ASC;
END;
GO

PRINT 'Stored procedure usp_GetCourseResultSummary created/updated';
GO

-- =============================================
-- Stored Procedure: usp_CreateCourseAttendanceDateWise
-- Description: Creates attendance record (uses correct table name)
-- =============================================
IF OBJECT_ID('dbo.usp_CreateCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CreateCourseAttendanceDateWise;
GO

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

        SELECT @AttendanceId AS AttendanceId, 'Attendance updated successfully' AS Message, CAST(1 AS BIT) AS Success;
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

    SELECT @AttendanceId AS AttendanceId, 'Attendance created successfully' AS Message, CAST(1 AS BIT) AS Success;
END;
GO

PRINT 'Stored procedure usp_CreateCourseAttendanceDateWise created/updated';
GO

-- =============================================
-- Stored Procedure: usp_UpdateCourseAttendanceDateWise
-- Description: Updates attendance record (uses correct table name)
-- =============================================
IF OBJECT_ID('dbo.usp_UpdateCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_UpdateCourseAttendanceDateWise;
GO

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
        SELECT CAST(0 AS BIT) AS Success, 'Attendance record not found' AS Message;
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

    SELECT CAST(1 AS BIT) AS Success, 'Attendance updated successfully' AS Message;
END;
GO

PRINT 'Stored procedure usp_UpdateCourseAttendanceDateWise created/updated';
GO

-- =============================================
-- Stored Procedure: usp_DeleteCourseAttendanceDateWise
-- Description: Soft deletes attendance record (uses correct table name)
-- =============================================
IF OBJECT_ID('dbo.usp_DeleteCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_DeleteCourseAttendanceDateWise;
GO

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
        SELECT CAST(0 AS BIT) AS Success, 'Attendance record not found' AS Message;
        RETURN;
    END

    -- Soft delete the record
    UPDATE [dbo].[CourseAttendance_DateWise]
    SET IsActive = 0,
        UpdatedDate = GETDATE(),
        UpdatedBy = @UpdatedBy
    WHERE AttendanceId = @AttendanceId
      AND TenantId = @TenantId;

    SELECT CAST(1 AS BIT) AS Success, 'Attendance deleted successfully' AS Message;
END;
GO

PRINT 'Stored procedure usp_DeleteCourseAttendanceDateWise created/updated';
GO

-- =============================================
-- Stored Procedure: usp_BulkMarkCourseAttendanceDateWise
-- Description: Bulk marks attendance (uses correct table name)
-- =============================================
IF OBJECT_ID('dbo.usp_BulkMarkCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_BulkMarkCourseAttendanceDateWise;
GO

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

        SELECT CAST(1 AS BIT) AS Success, 'Bulk attendance saved successfully' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT CAST(0 AS BIT) AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO

PRINT 'Stored procedure usp_BulkMarkCourseAttendanceDateWise created/updated';
GO

-- =============================================
-- Stored Procedure: usp_GetAttendanceSummaryByCoursePlan
-- Description: Gets attendance summary (uses correct table name)
-- =============================================
IF OBJECT_ID('dbo.usp_GetAttendanceSummaryByCoursePlan', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetAttendanceSummaryByCoursePlan;
GO

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

PRINT 'Stored procedure usp_GetAttendanceSummaryByCoursePlan created/updated';
GO

-- =============================================
-- Stored Procedure: usp_EnsureParticipantResult
-- Description: Creates result record for participant if it doesn't exist
-- =============================================
IF OBJECT_ID('dbo.usp_EnsureParticipantResult', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_EnsureParticipantResult;
GO

CREATE PROCEDURE [dbo].[usp_EnsureParticipantResult]
(
    @CoursePlanId INT,
    @StaffId INT,
    @TenantId INT,
    @CreatedBy INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Get course date range
    DECLARE @TotalDays INT;

    SELECT @TotalDays = DATEDIFF(DAY, StartDate, EndDate) + 1
    FROM [dbo].[CoursePlanning]
    WHERE Id = @CoursePlanId AND TenantId = @TenantId;

    -- Check if result exists
    IF NOT EXISTS (
        SELECT 1
        FROM [dbo].[CourseResult]
        WHERE CoursePlanId = @CoursePlanId
          AND StaffId = @StaffId
          AND TenantId = @TenantId
          AND IsActive = 1
    )
    BEGIN
        -- Create result record
        INSERT INTO [dbo].[CourseResult] (
            CoursePlanId, StaffId, TotalDays, PresentDays, AttendancePercentage,
            TenantId, IsActive, CreatedDate, CreatedBy
        )
        VALUES (
            @CoursePlanId, @StaffId, @TotalDays, 0, 0,
            @TenantId, 1, GETDATE(), @CreatedBy
        );
    END
END;
GO

PRINT 'Stored procedure usp_EnsureParticipantResult created/updated';
GO

PRINT '';
PRINT '=============================================';
PRINT 'All stored procedures created/updated successfully!';
PRINT 'Please run the application and test the Attendance and Results tabs.';
PRINT '=============================================';
