-- =============================================
-- Script: Update usp_GetCoursePlanningList Stored Procedure
-- Description: Updates the stored procedure to return all required fields
-- =============================================

-- Drop existing procedure if it exists
IF OBJECT_ID('dbo.usp_GetCoursePlanningList', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetCoursePlanningList;
GO

CREATE PROCEDURE [dbo].[usp_GetCoursePlanningList]
(
    @TenantId INT,
    @TrainerId INT = NULL,
    @CourseId INT = NULL,
    @IsActive BIT = NULL
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
    ORDER BY CP.StartDate DESC, CP.StartTime DESC;
END;
GO

PRINT 'Stored procedure usp_GetCoursePlanningList updated successfully';
