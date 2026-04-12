-- =============================================
-- Script: Add Course Completion Status to CoursePlanning
-- Description: Adds IsCompleted column and updates stored procedures
-- =============================================

-- Add IsCompleted column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[CoursePlanning]') AND name = 'IsCompleted')
BEGIN
    ALTER TABLE [dbo].[CoursePlanning] ADD IsCompleted BIT NOT NULL DEFAULT 0;
    PRINT 'Column IsCompleted added successfully';
END
ELSE
BEGIN
    PRINT 'Column IsCompleted already exists';
END
GO

-- Drop and recreate usp_GetCoursePlanningList
IF OBJECT_ID('dbo.usp_GetCoursePlanningList', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCoursePlanningList;
GO

CREATE PROCEDURE [dbo].[usp_GetCoursePlanningList]
(
    @TenantId INT,
    @TrainerId INT = NULL,
    @CourseId INT = NULL,
    @IsActive BIT = NULL,
    @IsCompleted BIT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CP.Id,
        CP.CourseId,
        CR.Code AS CourseCode,
        CR.CourseCode AS CourseNumber,
        CR.Title AS CourseTitle,
        CP.StartDate,
        CP.StartTime,
        CP.EndDate,
        CP.EndTime,
        CP.Venue,
        CP.TrainerId,
        S.Name AS TrainerName,
        S.Email AS TrainerEmail,
        CT.Name AS CourseType,
        CC.Name AS CourseCategory,
        CR.Duration AS CourseDuration,
        CP.IsActive,
        CP.IsCompleted,
        CP.CreatedDate,
        CP.CreatedBy,
        CP.UpdatedDate,
        CP.UpdatedBy,
        CP.Remarks,
        CP.QRCodePath,
        CP.UploadFilePaths,
        CP.TenantId
    FROM CoursePlanning CP
    INNER JOIN CourseRegistration CR ON CP.CourseId = CR.CourseId
    INNER JOIN Staff S ON CP.TrainerId = S.StaffId
    LEFT JOIN tbl_Master_Dropdown CT ON CR.CourseTypeId = CT.Id
    LEFT JOIN tbl_Master_Dropdown CC ON CR.CourseCategoryId = CC.Id
    WHERE CP.TenantId = @TenantId
      AND (@TrainerId IS NULL OR CP.TrainerId = @TrainerId)
      AND (@CourseId IS NULL OR CP.CourseId = @CourseId)
      AND (@IsActive IS NULL OR CP.IsActive = @IsActive)
      AND (@IsCompleted IS NULL OR CP.IsCompleted = @IsCompleted)
    ORDER BY CP.StartDate DESC, CP.StartTime DESC;
END;
GO

-- Drop and recreate usp_GetCoursePlanningById
IF OBJECT_ID('dbo.usp_GetCoursePlanningById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCoursePlanningById;
GO

CREATE PROCEDURE [dbo].[usp_GetCoursePlanningById]
    @Id INT,
    @TenantId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CP.Id,
        CP.CourseId,
        CP.StartDate,
        CP.StartTime,
        CP.EndDate,
        CP.EndTime,
        CP.Venue,
        CP.TrainerId,
        CP.Remarks,
        CP.UploadFilePaths,
        CP.QRCodePath,
        CP.TenantId,
        CP.IsActive,
        CP.IsCompleted,
        CP.CreatedDate,
        CP.CreatedBy,
        CP.UpdatedDate,
        CP.UpdatedBy,

        -- Course details
        CR.Title AS CourseTitle,
        CR.Code AS CourseCode,
        CR.CourseCode AS CourseNumber,
        CR.Duration AS CourseDuration,
        CR.TrainingModule,

        -- Trainer details
        S.Name AS TrainerName,
        S.Email AS TrainerEmail,
        S.PhoneNumber AS TrainerPhone,

        -- Course Type
        CT.Name AS CourseType,

        -- Course Category
        CC.Name AS CourseCategory

    FROM [dbo].[CoursePlanning] CP
    INNER JOIN [dbo].[CourseRegistration] CR ON CP.CourseId = CR.CourseId
    INNER JOIN [dbo].[Staff] S ON CP.TrainerId = S.StaffId
    LEFT JOIN [dbo].[tbl_Master_Dropdown] CT ON CR.CourseTypeId = CT.Id
    LEFT JOIN [dbo].[tbl_Master_Dropdown] CC ON CR.CourseCategoryId = CC.Id

    WHERE CP.Id = @Id
        AND CP.TenantId = @TenantId;
END;
GO

-- Create procedure to update completion status
IF OBJECT_ID('dbo.usp_UpdateCoursePlanningCompletionStatus', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_UpdateCoursePlanningCompletionStatus;
GO

CREATE PROCEDURE [dbo].[usp_UpdateCoursePlanningCompletionStatus]
    @Id INT,
    @TenantId INT,
    @IsCompleted BIT,
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[CoursePlanning]
    SET IsCompleted = @IsCompleted,
        UpdatedBy = @UpdatedBy,
        UpdatedDate = GETDATE()
    WHERE Id = @Id AND TenantId = @TenantId;

    SELECT @@ROWCOUNT AS RowsAffected;
END;
GO

PRINT 'Course completion status column and stored procedures created/updated successfully';
