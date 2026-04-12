-- =============================================
-- Script: Create LMS Stored Procedures
-- Description: Creates stored procedures for LMS data retrieval
-- =============================================

-- Drop existing procedure if it exists
IF OBJECT_ID('dbo.usp_GetLMSCourses', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetLMSCourses;
GO

CREATE PROCEDURE [dbo].[usp_GetLMSCourses]
(
    @TenantId INT,
    @SearchTerm NVARCHAR(200) = NULL,
    @CourseCategoryId INT = NULL,
    @SkillId INT = NULL,
    @DifficultyLevel NVARCHAR(20) = NULL,
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
    FROM [dbo].[LMSCourses] lc
    WHERE lc.TenantId = @TenantId
      AND (@IsActive IS NULL OR lc.IsActive = @IsActive)
      AND (@CourseCategoryId IS NULL OR lc.CourseCategoryId = @CourseCategoryId)
      AND (@SkillId IS NULL OR lc.SkillId = @SkillId)
      AND (@DifficultyLevel IS NULL OR lc.DifficultyLevel = @DifficultyLevel)
      AND (@SearchTerm IS NULL OR @SearchTerm = '' OR
           lc.Title LIKE '%' + @SearchTerm + '%' OR
           lc.Description LIKE '%' + @SearchTerm + '%' OR
           lc.Code LIKE '%' + @SearchTerm + '%');

    -- Get paginated courses list
    SELECT
        lc.CourseId,
        lc.Code,
        lc.Title,
        lc.Description,
        lc.CourseCategoryId,
        md.Name AS CourseCategoryName,
        lc.SkillId,
        ls.Name AS SkillName,
        lc.DurationHours,
        lc.DifficultyLevel,
        lc.IsActive,
        lc.CreatedDate,
        lc.CreatedBy,
        -- Enrollment count
        (SELECT COUNT(*) FROM LMSEnrollments le WHERE le.CourseId = lc.CourseId AND le.Status IN ('Enrolled', 'InProgress', 'Completed')) AS EnrollmentCount,
        -- Completion rate
        CASE
            WHEN (SELECT COUNT(*) FROM LMSEnrollments le WHERE le.CourseId = lc.CourseId AND le.Status IN ('Enrolled', 'InProgress', 'Completed')) > 0
            THEN CAST(ROUND((
                SELECT CAST(COUNT(*) AS DECIMAL(10,2))
                FROM LMSEnrollments le
                WHERE le.CourseId = lc.CourseId AND le.Status = 'Completed'
            ) / NULLIF((
                SELECT CAST(COUNT(*) AS DECIMAL(10,2))
                FROM LMSEnrollments le
                WHERE le.CourseId = lc.CourseId AND le.Status IN ('Enrolled', 'InProgress', 'Completed')
            ), 0) * 100, 2) AS DECIMAL(5,2))
            ELSE 0
        END AS CompletionRate
    FROM [dbo].[LMSCourses] lc
    LEFT JOIN [dbo].[MasterDropdown] md ON lc.CourseCategoryId = md.Id
    LEFT JOIN [dbo].[LMSSkills] ls ON lc.SkillId = ls.SkillId
    WHERE lc.TenantId = @TenantId
      AND (@IsActive IS NULL OR lc.IsActive = @IsActive)
      AND (@CourseCategoryId IS NULL OR lc.CourseCategoryId = @CourseCategoryId)
      AND (@SkillId IS NULL OR lc.SkillId = @SkillId)
      AND (@DifficultyLevel IS NULL OR lc.DifficultyLevel = @DifficultyLevel)
      AND (@SearchTerm IS NULL OR @SearchTerm = '' OR
           lc.Title LIKE '%' + @SearchTerm + '%' OR
           lc.Description LIKE '%' + @SearchTerm + '%' OR
           lc.Code LIKE '%' + @SearchTerm + '%')
    ORDER BY lc.CreatedDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Stored procedure for getting employee enrollments
IF OBJECT_ID('dbo.usp_GetLMSEnrollments', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetLMSEnrollments;
GO

CREATE PROCEDURE [dbo].[usp_GetLMSEnrollments]
(
    @TenantId INT,
    @StaffId INT = NULL,
    @CourseId INT = NULL,
    @Status NVARCHAR(20) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Get total count for pagination
    SELECT COUNT(*) AS TotalCount
    FROM [dbo].[LMSEnrollments] le
    INNER JOIN [dbo].[LMSCourses] lc ON le.CourseId = lc.CourseId
    INNER JOIN [dbo].[Staff] s ON le.StaffId = s.StaffId
    WHERE le.TenantId = @TenantId
      AND (@StaffId IS NULL OR le.StaffId = @StaffId)
      AND (@CourseId IS NULL OR le.CourseId = @CourseId)
      AND (@Status IS NULL OR le.Status = @Status);

    -- Get paginated enrollments
    SELECT
        le.EnrollmentId,
        le.CourseId,
        lc.Title AS CourseTitle,
        lc.Code AS CourseCode,
        lc.DurationHours,
        le.StaffId,
        s.Name AS StaffName,
        s.EmployeeCode,
        le.EnrollmentDate,
        le.CompletionDate,
        le.Status,
        le.ProgressPercentage,
        le.CertificateId,
        le.IsRecommended,
        le.CreatedDate,
        -- Calculate days since enrollment
        DATEDIFF(DAY, le.EnrollmentDate, GETUTCDATE()) AS DaysEnrolled,
        -- Module completion count
        (SELECT COUNT(*) FROM LMSProgress lp WHERE lp.EnrollmentId = le.EnrollmentId AND lp.IsCompleted = 1) AS CompletedModules,
        -- Total modules in course
        (SELECT COUNT(*) FROM LMSModules lm WHERE lm.CourseId = le.CourseId AND lm.IsActive = 1) AS TotalModules
    FROM [dbo].[LMSEnrollments] le
    INNER JOIN [dbo].[LMSCourses] lc ON le.CourseId = lc.CourseId
    INNER JOIN [dbo].[Staff] s ON le.StaffId = s.StaffId
    WHERE le.TenantId = @TenantId
      AND (@StaffId IS NULL OR le.StaffId = @StaffId)
      AND (@CourseId IS NULL OR le.CourseId = @CourseId)
      AND (@Status IS NULL OR le.Status = @Status)
    ORDER BY le.EnrollmentDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Stored procedure for LMS dashboard analytics
IF OBJECT_ID('dbo.usp_GetLMSDashboardAnalytics', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetLMSDashboardAnalytics;
GO

CREATE PROCEDURE [dbo].[usp_GetLMSDashboardAnalytics]
(
    @TenantId INT,
    @StaffId INT = NULL -- For employee-specific dashboard
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Total courses
    SELECT COUNT(*) AS TotalCourses
    FROM LMSCourses
    WHERE TenantId = @TenantId AND IsActive = 1;

    -- Total enrollments
    SELECT COUNT(*) AS TotalEnrollments
    FROM LMSEnrollments
    WHERE TenantId = @TenantId;

    -- Active enrollments (Enrolled or InProgress)
    SELECT COUNT(*) AS ActiveEnrollments
    FROM LMSEnrollments
    WHERE TenantId = @TenantId AND Status IN ('Enrolled', 'InProgress');

    -- Completed courses
    SELECT COUNT(*) AS CompletedCourses
    FROM LMSEnrollments
    WHERE TenantId = @TenantId AND Status = 'Completed';

    -- Certificates issued
    SELECT COUNT(*) AS CertificatesIssued
    FROM LMSCertificates
    WHERE TenantId = @TenantId AND IsActive = 1;

    -- Average completion rate
    SELECT
        CASE
            WHEN COUNT(*) > 0 THEN CAST(AVG(ProgressPercentage) AS DECIMAL(5,2))
            ELSE 0
        END AS AverageCompletionRate
    FROM LMSEnrollments
    WHERE TenantId = @TenantId AND Status IN ('Enrolled', 'InProgress', 'Completed');

    -- Monthly enrollment trend (last 12 months)
    SELECT
        YEAR(EnrollmentDate) AS Year,
        MONTH(EnrollmentDate) AS Month,
        COUNT(*) AS EnrollmentCount
    FROM LMSEnrollments
    WHERE TenantId = @TenantId
      AND EnrollmentDate >= DATEADD(MONTH, -12, GETUTCDATE())
    GROUP BY YEAR(EnrollmentDate), MONTH(EnrollmentDate)
    ORDER BY Year, Month;

    -- Top courses by enrollment
    SELECT TOP 10
        lc.CourseId,
        lc.Title,
        lc.Code,
        COUNT(le.EnrollmentId) AS EnrollmentCount
    FROM LMSCourses lc
    LEFT JOIN LMSEnrollments le ON lc.CourseId = le.CourseId
    WHERE lc.TenantId = @TenantId AND lc.IsActive = 1
    GROUP BY lc.CourseId, lc.Title, lc.Code
    ORDER BY EnrollmentCount DESC;

    -- Skill gap analysis (if StaffId provided)
    IF @StaffId IS NOT NULL
    BEGIN
        -- Employee's current skills
        SELECT
            es.SkillId,
            s.Name AS SkillName,
            es.ProficiencyLevel,
            es.LastAssessedDate
        FROM LMSEmployeeSkills es
        INNER JOIN LMSSkills s ON es.SkillId = s.SkillId
        WHERE es.StaffId = @StaffId AND es.TenantId = @TenantId AND es.IsActive = 1;

        -- Recommended courses for employee
        SELECT TOP 5
            lr.RecommendationId,
            lc.CourseId,
            lc.Title,
            lc.Code,
            lr.RecommendationReason,
            lr.RecommendationScore
        FROM LMSRecommendations lr
        INNER JOIN LMSCourses lc ON lr.CourseId = lc.CourseId
        WHERE lr.StaffId = @StaffId AND lr.TenantId = @TenantId
          AND lr.IsAccepted IS NULL -- Not yet accepted/rejected
        ORDER BY lr.RecommendationScore DESC;
    END
END
GO

-- Stored procedure for course recommendations
IF OBJECT_ID('dbo.usp_GetLMSRecommendations', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetLMSRecommendations;
GO

CREATE PROCEDURE [dbo].[usp_GetLMSRecommendations]
(
    @TenantId INT,
    @StaffId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    -- Get employee's position and department from Staff table
    DECLARE @Position NVARCHAR(100);
    DECLARE @Department NVARCHAR(100);

    SELECT @Position = Position, @Department = Department
    FROM Staff
    WHERE StaffId = @StaffId AND TenantId = @TenantId;

    -- Get employee's current skills
    SELECT es.SkillId, s.Name AS SkillName, es.ProficiencyLevel
    INTO #EmployeeSkills
    FROM LMSEmployeeSkills es
    INNER JOIN LMSSkills s ON es.SkillId = s.SkillId
    WHERE es.StaffId = @StaffId AND es.TenantId = @TenantId AND es.IsActive = 1;

    -- Find courses that match employee's position/department or complement their skills
    SELECT DISTINCT
        lc.CourseId,
        lc.Title,
        lc.Description,
        lc.DurationHours,
        lc.DifficultyLevel,
        ls.Name AS SkillName,
        CASE
            WHEN lc.SkillId IN (SELECT SkillId FROM #EmployeeSkills WHERE ProficiencyLevel IN ('Beginner', 'Intermediate')) THEN 'Skill Enhancement'
            WHEN lc.Title LIKE '%' + @Position + '%' THEN 'Position-based'
            WHEN lc.Description LIKE '%' + @Department + '%' THEN 'Department-based'
            ELSE 'General Recommendation'
        END AS RecommendationReason,
        CASE
            WHEN lc.SkillId IN (SELECT SkillId FROM #EmployeeSkills WHERE ProficiencyLevel IN ('Beginner', 'Intermediate')) THEN 90
            WHEN lc.Title LIKE '%' + @Position + '%' THEN 80
            WHEN lc.Description LIKE '%' + @Department + '%' THEN 70
            ELSE 50
        END AS RecommendationScore
    FROM LMSCourses lc
    LEFT JOIN LMSSkills ls ON lc.SkillId = ls.SkillId
    WHERE lc.TenantId = @TenantId
      AND lc.IsActive = 1
      -- Exclude courses already enrolled or completed
      AND lc.CourseId NOT IN (
          SELECT CourseId FROM LMSEnrollments
          WHERE StaffId = @StaffId AND Status IN ('Enrolled', 'InProgress', 'Completed')
      )
    ORDER BY RecommendationScore DESC, lc.CreatedDate DESC;

    DROP TABLE #EmployeeSkills;
END
GO

PRINT 'LMS stored procedures created successfully';