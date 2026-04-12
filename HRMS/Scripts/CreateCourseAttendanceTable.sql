-- =============================================
-- Script: Create Course_Attendance Table and Stored Procedures
-- Description: Creates the Course_Attendance table and related stored procedures
-- =============================================

-- Drop existing stored procedures if they exist
IF OBJECT_ID('dbo.usp_GetCourseAttendance', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCourseAttendance;
GO

IF OBJECT_ID('dbo.usp_MarkAttendance', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_MarkAttendance;
GO

IF OBJECT_ID('dbo.usp_GetAttendanceByCoursePlan', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetAttendanceByCoursePlan;
GO

-- Create Course_Attendance table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Course_Attendance')
BEGIN
    CREATE TABLE [dbo].[Course_Attendance] (
        AttendanceId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        CoursePlanId INT NOT NULL,
        StaffId INT NOT NULL,
        AttendanceDate DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        CheckInTime TIME(7) NULL,
        CheckOutTime TIME(7) NULL,
        Status NVARCHAR(50) NULL DEFAULT 'Present',
        Remarks NVARCHAR(500) NULL,
        TenantId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        CreatedBy INT NULL,
        UpdatedDate DATETIME2(7) NULL,
        UpdatedBy INT NULL,

        -- Foreign key constraints
        CONSTRAINT FK_CourseAttendance_CoursePlanning FOREIGN KEY (CoursePlanId)
            REFERENCES [dbo].[CoursePlanning](Id),
        CONSTRAINT FK_CourseAttendance_User FOREIGN KEY (UserId)
            REFERENCES [dbo].[Users](UserId),
        CONSTRAINT FK_CourseAttendance_Staff FOREIGN KEY (StaffId)
            REFERENCES [dbo].[Staff](StaffId)
    );

    -- Create indexes for better query performance
    CREATE NONCLUSTERED INDEX IX_CourseAttendance_TenantId
        ON [dbo].[Course_Attendance](TenantId);

    CREATE NONCLUSTERED INDEX IX_CourseAttendance_CoursePlanId
        ON [dbo].[Course_Attendance](CoursePlanId);

    CREATE NONCLUSTERED INDEX IX_CourseAttendance_UserId
        ON [dbo].[Course_Attendance](UserId);

    CREATE NONCLUSTERED INDEX IX_CourseAttendance_StaffId
        ON [dbo].[Course_Attendance](StaffId);

    CREATE NONCLUSTERED INDEX IX_CourseAttendance_AttendanceDate
        ON [dbo].[Course_Attendance](AttendanceDate);

    PRINT 'Table Course_Attendance created successfully';
END
ELSE
BEGIN
    PRINT 'Table Course_Attendance already exists';
END
GO

-- =============================================
-- Stored Procedure: usp_MarkAttendance
-- Description: Marks attendance for a user in a course plan (prevents duplicates)
-- Parameters:
--   @UserId - User ID
--   @CoursePlanId - Course planning ID
--   @StaffId - Staff ID
--   @TenantId - Tenant ID
--   @CheckInTime - Check-in time
--   @CreatedBy - User who created the record
-- Returns: AttendanceId if successful, -1 if duplicate
-- =============================================
CREATE PROCEDURE [dbo].[usp_MarkAttendance]
    @UserId INT,
    @CoursePlanId INT,
    @StaffId INT,
    @TenantId INT,
    @CheckInTime TIME(7) = NULL,
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AttendanceId INT = -1;
    DECLARE @Today DATE = CAST(GETDATE() AS DATE);

    -- Check if attendance already exists for today
    IF EXISTS (
        SELECT 1
        FROM [dbo].[Course_Attendance]
        WHERE UserId = @UserId
            AND CoursePlanId = @CoursePlanId
            AND TenantId = @TenantId
            AND CAST(AttendanceDate AS DATE) = @Today
            AND IsActive = 1
    )
    BEGIN
        -- Return -1 to indicate duplicate
        SELECT -1 AS AttendanceId, 'Attendance already marked for today' AS Message;
        RETURN;
    END

    -- Insert new attendance record
    INSERT INTO [dbo].[Course_Attendance] (
        UserId,
        CoursePlanId,
        StaffId,
        AttendanceDate,
        CheckInTime,
        Status,
        TenantId,
        IsActive,
        CreatedDate,
        CreatedBy
    )
    VALUES (
        @UserId,
        @CoursePlanId,
        @StaffId,
        GETDATE(),
        ISNULL(@CheckInTime, CAST(GETDATE() AS TIME)),
        'Present',
        @TenantId,
        1,
        GETDATE(),
        @CreatedBy
    );

    SET @AttendanceId = SCOPE_IDENTITY();

    SELECT @AttendanceId AS AttendanceId, 'Attendance marked successfully' AS Message;
END
GO

-- =============================================
-- Stored Procedure: usp_GetCourseAttendance
-- Description: Retrieves attendance records with course and staff details
-- Parameters:
--   @TenantId - Filter by tenant
--   @CoursePlanId - Optional filter by course plan (NULL = all)
--   @UserId - Optional filter by user (NULL = all)
--   @FromDate - Optional filter by date from
--   @ToDate - Optional filter by date to
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetCourseAttendance]
    @TenantId INT,
    @CoursePlanId INT = NULL,
    @UserId INT = NULL,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ca.AttendanceId,
        ca.UserId,
        ca.CoursePlanId,
        ca.StaffId,
        ca.AttendanceDate,
        ca.CheckInTime,
        ca.CheckOutTime,
        ca.Status,
        ca.Remarks,
        ca.TenantId,
        ca.IsActive,
        ca.CreatedDate,
        ca.CreatedBy,
        ca.UpdatedDate,
        ca.UpdatedBy,

        -- User details
        u.FullName AS UserName,
        u.Email AS UserEmail,

        -- Staff details
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Position,

        -- Course Plan details
        cp.StartDate AS CourseStartDate,
        cp.EndDate AS CourseEndDate,
        cp.Venue,

        -- Course details
        cr.Title AS CourseTitle,
        cr.Code AS CourseCode,
        cr.CourseCode AS CourseNumber,

        -- Trainer details
        trainer.Name AS TrainerName

    FROM [dbo].[Course_Attendance] ca
    INNER JOIN [dbo].[Users] u ON ca.UserId = u.UserId
    INNER JOIN [dbo].[Staff] s ON ca.StaffId = s.StaffId
    INNER JOIN [dbo].[CoursePlanning] cp ON ca.CoursePlanId = cp.Id
    INNER JOIN [dbo].[CourseRegistration] cr ON cp.CourseId = cr.CourseId
    INNER JOIN [dbo].[Staff] trainer ON cp.TrainerId = trainer.StaffId

    WHERE ca.TenantId = @TenantId
        AND (@CoursePlanId IS NULL OR ca.CoursePlanId = @CoursePlanId)
        AND (@UserId IS NULL OR ca.UserId = @UserId)
        AND (@FromDate IS NULL OR CAST(ca.AttendanceDate AS DATE) >= @FromDate)
        AND (@ToDate IS NULL OR CAST(ca.AttendanceDate AS DATE) <= @ToDate)
        AND ca.IsActive = 1

    ORDER BY ca.AttendanceDate DESC, ca.CheckInTime DESC;
END
GO

-- =============================================
-- Stored Procedure: usp_GetAttendanceByCoursePlan
-- Description: Retrieves attendance summary for a specific course plan
-- Parameters:
--   @CoursePlanId - Course planning ID
--   @TenantId - Tenant ID
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetAttendanceByCoursePlan]
    @CoursePlanId INT,
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ca.AttendanceId,
        ca.AttendanceDate,
        ca.CheckInTime,
        ca.CheckOutTime,
        ca.Status,
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Position,
        s.Email,
        u.FullName AS UserName,
        u.Email AS UserEmail,
        ca.Remarks

    FROM [dbo].[Course_Attendance] ca
    INNER JOIN [dbo].[Users] u ON ca.UserId = u.UserId
    INNER JOIN [dbo].[Staff] s ON ca.StaffId = s.StaffId

    WHERE ca.CoursePlanId = @CoursePlanId
        AND ca.TenantId = @TenantId
        AND ca.IsActive = 1

    ORDER BY ca.AttendanceDate DESC, s.Name ASC;
END
GO

PRINT 'Course_Attendance table and stored procedures created successfully';
