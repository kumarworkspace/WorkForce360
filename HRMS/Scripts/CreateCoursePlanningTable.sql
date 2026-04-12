-- =============================================
-- Script: Create CoursePlanning Table and Stored Procedures
-- Description: Creates the CoursePlanning table for TMS module with required stored procedures
-- =============================================

-- Drop existing stored procedures if they exist
IF OBJECT_ID('dbo.usp_GetCoursePlanningList', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCoursePlanningList;
GO

IF OBJECT_ID('dbo.usp_GetCoursePlanningById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCoursePlanningById;
GO

IF OBJECT_ID('dbo.usp_ValidateCoursePlanningConflict', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_ValidateCoursePlanningConflict;
GO

-- Create CoursePlanning table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CoursePlanning')
BEGIN
    CREATE TABLE [dbo].[CoursePlanning] (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CourseId INT NOT NULL,
        StartDate DATE NOT NULL,
        StartTime TIME(7) NOT NULL,
        EndDate DATE NOT NULL,
        EndTime TIME(7) NOT NULL,
        Venue NVARCHAR(200) NOT NULL,
        TrainerId INT NOT NULL,
        Remarks NVARCHAR(500) NULL,
        UploadFilePaths NVARCHAR(MAX) NULL,
        QRCodePath NVARCHAR(500) NULL,
        TenantId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2(7) NOT NULL DEFAULT GETDATE(),
        CreatedBy INT NULL,
        UpdatedDate DATETIME2(7) NULL,
        UpdatedBy INT NULL,

        -- Foreign key constraints
        CONSTRAINT FK_CoursePlanning_CourseRegistration FOREIGN KEY (CourseId)
            REFERENCES [dbo].[CourseRegistration](CourseId),
        CONSTRAINT FK_CoursePlanning_Staff FOREIGN KEY (TrainerId)
            REFERENCES [dbo].[Staff](StaffId)
    );

    -- Create indexes for better query performance
    CREATE NONCLUSTERED INDEX IX_CoursePlanning_TenantId
        ON [dbo].[CoursePlanning](TenantId);

    CREATE NONCLUSTERED INDEX IX_CoursePlanning_CourseId
        ON [dbo].[CoursePlanning](CourseId);

    CREATE NONCLUSTERED INDEX IX_CoursePlanning_TrainerId
        ON [dbo].[CoursePlanning](TrainerId);

    CREATE NONCLUSTERED INDEX IX_CoursePlanning_StartDate
        ON [dbo].[CoursePlanning](StartDate);

    PRINT 'Table CoursePlanning created successfully';
END
ELSE
BEGIN
    PRINT 'Table CoursePlanning already exists';
END
GO

-- =============================================
-- Stored Procedure: usp_GetCoursePlanningList
-- Description: Retrieves a filtered list of course planning records
-- Parameters:
--   @TenantId - Filter by tenant
--   @TrainerId - Optional filter by trainer (NULL = all trainers)
--   @CourseId - Optional filter by course (NULL = all courses)
--   @IsActive - Optional filter by active status (NULL = all)
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetCoursePlanningList]
    @TenantId INT,
    @TrainerId INT = NULL,
    @CourseId INT = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cp.Id,
        cp.CourseId,
        cp.StartDate,
        cp.StartTime,
        cp.EndDate,
        cp.EndTime,
        cp.Venue,
        cp.TrainerId,
        cp.Remarks,
        cp.UploadFilePaths,
        cp.QRCodePath,
        cp.TenantId,
        cp.IsActive,
        cp.CreatedDate,
        cp.CreatedBy,
        cp.UpdatedDate,
        cp.UpdatedBy,

        -- Course details
        cr.Title AS CourseTitle,
        cr.Code AS CourseCode,
        cr.CourseCode AS CourseNumber,
        cr.Duration AS CourseDuration,

        -- Trainer details
        s.StaffName AS TrainerName,
        s.Email AS TrainerEmail,

        -- Course Type
        ct.DropdownValue AS CourseType,

        -- Course Category
        cc.DropdownValue AS CourseCategory

    FROM [dbo].[CoursePlanning] cp
    INNER JOIN [dbo].[CourseRegistration] cr ON cp.CourseId = cr.CourseId
    INNER JOIN [dbo].[Staff] s ON cp.TrainerId = s.StaffId
    LEFT JOIN [dbo].[MasterDropdown] ct ON cr.CourseTypeId = ct.DropdownId
    LEFT JOIN [dbo].[MasterDropdown] cc ON cr.CourseCategoryId = cc.DropdownId

    WHERE cp.TenantId = @TenantId
        AND (@TrainerId IS NULL OR cp.TrainerId = @TrainerId)
        AND (@CourseId IS NULL OR cp.CourseId = @CourseId)
        AND (@IsActive IS NULL OR cp.IsActive = @IsActive)

    ORDER BY cp.StartDate DESC, cp.StartTime DESC;
END
GO

-- =============================================
-- Stored Procedure: usp_GetCoursePlanningById
-- Description: Retrieves a single course planning record by ID
-- Parameters:
--   @Id - Course planning ID
--   @TenantId - Tenant ID for security
-- =============================================
CREATE PROCEDURE [dbo].[usp_GetCoursePlanningById]
    @Id INT,
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cp.Id,
        cp.CourseId,
        cp.StartDate,
        cp.StartTime,
        cp.EndDate,
        cp.EndTime,
        cp.Venue,
        cp.TrainerId,
        cp.Remarks,
        cp.UploadFilePaths,
        cp.QRCodePath,
        cp.TenantId,
        cp.IsActive,
        cp.CreatedDate,
        cp.CreatedBy,
        cp.UpdatedDate,
        cp.UpdatedBy,

        -- Course details
        cr.Title AS CourseTitle,
        cr.Code AS CourseCode,
        cr.CourseCode AS CourseNumber,
        cr.Duration AS CourseDuration,
        cr.TrainingModule,

        -- Trainer details
        s.StaffName AS TrainerName,
        s.Email AS TrainerEmail,
        s.PhoneNo AS TrainerPhone,

        -- Course Type
        ct.DropdownValue AS CourseType,

        -- Course Category
        cc.DropdownValue AS CourseCategory

    FROM [dbo].[CoursePlanning] cp
    INNER JOIN [dbo].[CourseRegistration] cr ON cp.CourseId = cr.CourseId
    INNER JOIN [dbo].[Staff] s ON cp.TrainerId = s.StaffId
    LEFT JOIN [dbo].[MasterDropdown] ct ON cr.CourseTypeId = ct.DropdownId
    LEFT JOIN [dbo].[MasterDropdown] cc ON cr.CourseCategoryId = cc.DropdownId

    WHERE cp.Id = @Id
        AND cp.TenantId = @TenantId;
END
GO

-- =============================================
-- Stored Procedure: usp_ValidateCoursePlanningConflict
-- Description: Validates if there are any scheduling conflicts
-- Parameters:
--   @Id - Course planning ID (NULL for new records)
--   @TrainerId - Trainer ID to check
--   @StartDate - Planned start date
--   @StartTime - Planned start time
--   @EndDate - Planned end date
--   @EndTime - Planned end time
--   @TenantId - Tenant ID
-- Returns: Count of conflicts (0 = no conflicts)
-- =============================================
CREATE PROCEDURE [dbo].[usp_ValidateCoursePlanningConflict]
    @Id INT = NULL,
    @TrainerId INT,
    @StartDate DATE,
    @StartTime TIME(7),
    @EndDate DATE,
    @EndTime TIME(7),
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check for overlapping schedules for the same trainer
    -- A conflict exists if:
    -- 1. The trainer is the same
    -- 2. The date/time ranges overlap
    -- 3. Both records are active
    -- 4. Not the same record (for updates)

    SELECT COUNT(*) AS ConflictCount
    FROM [dbo].[CoursePlanning] cp
    WHERE cp.TenantId = @TenantId
        AND cp.TrainerId = @TrainerId
        AND cp.IsActive = 1
        AND (@Id IS NULL OR cp.Id != @Id)
        AND (
            -- Check if the new schedule overlaps with existing schedules
            -- Case 1: New schedule starts during an existing schedule
            (@StartDate BETWEEN cp.StartDate AND cp.EndDate
                AND (cp.StartDate != @StartDate OR @StartTime < cp.EndTime)
                AND (cp.EndDate != @StartDate OR @StartTime < cp.EndTime))
            OR
            -- Case 2: New schedule ends during an existing schedule
            (@EndDate BETWEEN cp.StartDate AND cp.EndDate
                AND (cp.StartDate != @EndDate OR @EndTime > cp.StartTime)
                AND (cp.EndDate != @EndDate OR @EndTime > cp.StartTime))
            OR
            -- Case 3: New schedule completely encompasses an existing schedule
            (@StartDate <= cp.StartDate AND @EndDate >= cp.EndDate)
            OR
            -- Case 4: Existing schedule completely encompasses the new schedule
            (cp.StartDate <= @StartDate AND cp.EndDate >= @EndDate)
        );
END
GO

PRINT 'CoursePlanning table and stored procedures created successfully';
