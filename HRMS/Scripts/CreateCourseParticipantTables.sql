-- =============================================
-- Script: Create Course Participant, Attendance, and Result Tables
-- Description: Creates tables and stored procedures for course participant management
-- =============================================

-- =============================================
-- 1. CourseParticipant Table
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

        -- Foreign key constraints
        CONSTRAINT FK_CourseParticipant_CoursePlanning FOREIGN KEY (CoursePlanId)
            REFERENCES [dbo].[CoursePlanning](Id),
        CONSTRAINT FK_CourseParticipant_Staff FOREIGN KEY (StaffId)
            REFERENCES [dbo].[Staff](StaffId),

        -- Unique constraint to prevent duplicates
        CONSTRAINT UQ_CourseParticipant_Course_Staff UNIQUE (CoursePlanId, StaffId, TenantId)
    );

    -- Create indexes for better query performance
    CREATE NONCLUSTERED INDEX IX_CourseParticipant_TenantId
        ON [dbo].[CourseParticipant](TenantId);

    CREATE NONCLUSTERED INDEX IX_CourseParticipant_CoursePlanId
        ON [dbo].[CourseParticipant](CoursePlanId);

    CREATE NONCLUSTERED INDEX IX_CourseParticipant_StaffId
        ON [dbo].[CourseParticipant](StaffId);

    PRINT 'Table CourseParticipant created successfully';
END
ELSE
BEGIN
    PRINT 'Table CourseParticipant already exists';
END
GO

-- =============================================
-- 2. CourseAttendance Table (Date-wise attendance)
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

        -- Foreign key constraints
        CONSTRAINT FK_CourseAttendanceDateWise_CoursePlanning FOREIGN KEY (CoursePlanId)
            REFERENCES [dbo].[CoursePlanning](Id),
        CONSTRAINT FK_CourseAttendanceDateWise_Staff FOREIGN KEY (StaffId)
            REFERENCES [dbo].[Staff](StaffId),

        -- Unique constraint to prevent duplicates
        CONSTRAINT UQ_CourseAttendanceDateWise_Course_Staff_Date UNIQUE (CoursePlanId, StaffId, AttendanceDate, TenantId)
    );

    -- Create indexes for better query performance
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
ELSE
BEGIN
    PRINT 'Table CourseAttendance_DateWise already exists';
END
GO

-- =============================================
-- 3. CourseResult Table
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
        ResultStatus NVARCHAR(10) NULL, -- Pass / Fail
        CertificatePath NVARCHAR(500) NULL,
        TenantId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        CreatedBy INT NULL,
        UpdatedDate DATETIME2(7) NULL,
        UpdatedBy INT NULL,

        -- Foreign key constraints
        CONSTRAINT FK_CourseResult_CoursePlanning FOREIGN KEY (CoursePlanId)
            REFERENCES [dbo].[CoursePlanning](Id),
        CONSTRAINT FK_CourseResult_Staff FOREIGN KEY (StaffId)
            REFERENCES [dbo].[Staff](StaffId),

        -- Unique constraint
        CONSTRAINT UQ_CourseResult_Course_Staff UNIQUE (CoursePlanId, StaffId, TenantId)
    );

    -- Create indexes for better query performance
    CREATE NONCLUSTERED INDEX IX_CourseResult_TenantId
        ON [dbo].[CourseResult](TenantId);

    CREATE NONCLUSTERED INDEX IX_CourseResult_CoursePlanId
        ON [dbo].[CourseResult](CoursePlanId);

    CREATE NONCLUSTERED INDEX IX_CourseResult_StaffId
        ON [dbo].[CourseResult](StaffId);

    CREATE NONCLUSTERED INDEX IX_CourseResult_ResultStatus
        ON [dbo].[CourseResult](ResultStatus);

    PRINT 'Table CourseResult created successfully';
END
ELSE
BEGIN
    PRINT 'Table CourseResult already exists';
END
GO

-- =============================================
-- Stored Procedure: usp_GetCourseDetails
-- =============================================
IF OBJECT_ID('dbo.usp_GetCourseDetails', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCourseDetails;
GO

CREATE PROCEDURE [dbo].[usp_GetCourseDetails]
    @CoursePlanId INT,
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cp.Id AS CoursePlanId,
        cp.CourseId,
        cp.StartDate,
        cp.EndDate,
        cp.StartTime,
        cp.EndTime,
        cp.Venue,
        cp.TrainerId,
        cp.Remarks,
        cp.TenantId,
        cp.IsActive,

        -- Course details
        cr.Title AS CourseTitle,
        cr.Code AS CourseCode,
        cr.CourseCode AS CourseNumber,

        -- Trainer details
        trainer.Name AS TrainerName,
        trainer.Email AS TrainerEmail,
        trainer.PhoneNumber AS TrainerPhone

    FROM [dbo].[CoursePlanning] cp
    INNER JOIN [dbo].[CourseRegistration] cr ON cp.CourseId = cr.CourseId
    INNER JOIN [dbo].[Staff] trainer ON cp.TrainerId = trainer.StaffId

    WHERE cp.Id = @CoursePlanId
        AND cp.TenantId = @TenantId
        AND cp.IsActive = 1;
END
GO

-- =============================================
-- Stored Procedure: usp_GetCourseParticipants
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
END
GO

-- =============================================
-- Stored Procedure: usp_GetCourseAttendance
-- =============================================
IF OBJECT_ID('dbo.usp_GetCourseAttendanceDateWise', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCourseAttendanceDateWise;
GO

CREATE PROCEDURE [dbo].[usp_GetCourseAttendanceDateWise]
    @CoursePlanId INT,
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ca.AttendanceId,
        ca.CoursePlanId,
        ca.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        ca.AttendanceDate,
        ca.IsPresent,
        ca.Remarks

    FROM [dbo].[CourseAttendance_DateWise] ca
    INNER JOIN [dbo].[Staff] s ON ca.StaffId = s.StaffId

    WHERE ca.CoursePlanId = @CoursePlanId
        AND ca.TenantId = @TenantId
        AND ca.IsActive = 1

    ORDER BY s.Name ASC, ca.AttendanceDate ASC;
END
GO

-- =============================================
-- Stored Procedure: usp_GetCourseResultSummary
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

    SELECT
        cr.ResultId,
        cr.CoursePlanId,
        cr.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        s.Department,
        s.Position,
        cr.TotalDays,
        cr.PresentDays,
        cr.AttendancePercentage,
        cr.ResultStatus,
        cr.CertificatePath,
        cr.UpdatedDate

    FROM [dbo].[CourseResult] cr
    INNER JOIN [dbo].[Staff] s ON cr.StaffId = s.StaffId

    WHERE cr.CoursePlanId = @CoursePlanId
        AND cr.TenantId = @TenantId
        AND cr.IsActive = 1

    ORDER BY s.Name ASC;
END
GO

PRINT 'Course Participant tables and stored procedures created successfully';
