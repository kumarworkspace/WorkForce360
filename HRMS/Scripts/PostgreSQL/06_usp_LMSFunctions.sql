-- =============================================
-- PostgreSQL Functions: LMS stored procedures
-- =============================================

-- =============================================
-- Function: usp_GetLMSCourses
-- =============================================
DROP FUNCTION IF EXISTS usp_GetLMSCourses(INT, VARCHAR, INT, INT, VARCHAR, BOOLEAN, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetLMSCourses(
    p_tenant_id        INT,
    p_search_term      VARCHAR(200) DEFAULT NULL,
    p_course_category_id INT        DEFAULT NULL,
    p_skill_id         INT          DEFAULT NULL,
    p_difficulty_level VARCHAR(20)  DEFAULT NULL,
    p_is_active        BOOLEAN      DEFAULT TRUE,
    p_page_number      INT          DEFAULT 1,
    p_page_size        INT          DEFAULT 10
)
RETURNS TABLE (
    "TotalCount"      INT,
    "CourseId"        INT,
    "Code"            VARCHAR,
    "Title"           VARCHAR,
    "Description"     TEXT,
    "CourseCategoryId" INT,
    "CourseCategoryName" VARCHAR,
    "SkillId"         INT,
    "SkillName"       VARCHAR,
    "DurationHours"   INT,
    "DifficultyLevel" VARCHAR,
    "IsActive"        BOOLEAN,
    "CreatedDate"     TIMESTAMP,
    "CreatedBy"       INT,
    "EnrollmentCount" INT,
    "CompletionRate"  NUMERIC(5,2)
)
LANGUAGE plpgsql AS $$
DECLARE
    v_offset INT := (p_page_number - 1) * p_page_size;
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) OVER()::INT AS "TotalCount",
        lc."CourseId",
        lc."Code",
        lc."Title",
        lc."Description",
        lc."CourseCategoryId",
        md."Name"            AS "CourseCategoryName",
        lc."SkillId",
        ls."Name"            AS "SkillName",
        lc."DurationHours",
        lc."DifficultyLevel",
        lc."IsActive",
        lc."CreatedDate",
        lc."CreatedBy",
        (SELECT COUNT(*)::INT
         FROM "LMSEnrollments" le2
         WHERE le2."CourseId" = lc."CourseId"
           AND le2."Status" IN ('Enrolled','InProgress','Completed'))  AS "EnrollmentCount",
        CASE
            WHEN (SELECT COUNT(*) FROM "LMSEnrollments" le3
                  WHERE le3."CourseId" = lc."CourseId"
                    AND le3."Status" IN ('Enrolled','InProgress','Completed')) > 0
            THEN CAST(ROUND(
                    (SELECT COUNT(*)::NUMERIC FROM "LMSEnrollments" le4
                     WHERE le4."CourseId" = lc."CourseId" AND le4."Status" = 'Completed')
                    / NULLIF(
                        (SELECT COUNT(*)::NUMERIC FROM "LMSEnrollments" le5
                         WHERE le5."CourseId" = lc."CourseId"
                           AND le5."Status" IN ('Enrolled','InProgress','Completed')),
                    0) * 100, 2) AS NUMERIC(5,2))
            ELSE 0::NUMERIC(5,2)
        END                  AS "CompletionRate"
    FROM "LMSCourses" lc
    LEFT JOIN "MasterDropdown" md ON lc."CourseCategoryId" = md."Id"
    LEFT JOIN "LMSSkills" ls      ON lc."SkillId"          = ls."SkillId"
    WHERE lc."TenantId" = p_tenant_id
      AND (p_is_active           IS NULL OR lc."IsActive"         = p_is_active)
      AND (p_course_category_id  IS NULL OR lc."CourseCategoryId" = p_course_category_id)
      AND (p_skill_id            IS NULL OR lc."SkillId"          = p_skill_id)
      AND (p_difficulty_level    IS NULL OR lc."DifficultyLevel"  = p_difficulty_level)
      AND (p_search_term IS NULL OR p_search_term = '' OR
           lc."Title"       ILIKE '%' || p_search_term || '%' OR
           lc."Description" ILIKE '%' || p_search_term || '%' OR
           lc."Code"        ILIKE '%' || p_search_term || '%')
    ORDER BY lc."CreatedDate" DESC
    OFFSET v_offset
    LIMIT  p_page_size;
END;
$$;

-- =============================================
-- Function: usp_GetLMSEnrollments
-- =============================================
DROP FUNCTION IF EXISTS usp_GetLMSEnrollments(INT, INT, INT, VARCHAR, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetLMSEnrollments(
    p_tenant_id   INT,
    p_staff_id    INT     DEFAULT NULL,
    p_course_id   INT     DEFAULT NULL,
    p_status      VARCHAR(20) DEFAULT NULL,
    p_page_number INT     DEFAULT 1,
    p_page_size   INT     DEFAULT 10
)
RETURNS TABLE (
    "TotalCount"         INT,
    "EnrollmentId"       INT,
    "CourseId"           INT,
    "CourseTitle"        VARCHAR,
    "CourseCode"         VARCHAR,
    "DurationHours"      INT,
    "StaffId"            INT,
    "StaffName"          VARCHAR,
    "EmployeeCode"       VARCHAR,
    "EnrollmentDate"     TIMESTAMP,
    "CompletionDate"     TIMESTAMP,
    "Status"             VARCHAR,
    "ProgressPercentage" NUMERIC,
    "CertificateId"      INT,
    "IsRecommended"      BOOLEAN,
    "CreatedDate"        TIMESTAMP,
    "DaysEnrolled"       INT,
    "CompletedModules"   INT,
    "TotalModules"       INT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_offset INT := (p_page_number - 1) * p_page_size;
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) OVER()::INT AS "TotalCount",
        le."EnrollmentId",
        le."CourseId",
        lc."Title"           AS "CourseTitle",
        lc."Code"            AS "CourseCode",
        lc."DurationHours",
        le."StaffId",
        s."Name"             AS "StaffName",
        s."EmployeeCode",
        le."EnrollmentDate",
        le."CompletionDate",
        le."Status",
        le."ProgressPercentage",
        le."CertificateId",
        le."IsRecommended",
        le."CreatedDate",
        (NOW()::DATE - le."EnrollmentDate"::DATE)::INT AS "DaysEnrolled",
        (SELECT COUNT(*)::INT FROM "LMSProgress" lp
         WHERE lp."EnrollmentId" = le."EnrollmentId" AND lp."IsCompleted" = TRUE)  AS "CompletedModules",
        (SELECT COUNT(*)::INT FROM "LMSModules" lm
         WHERE lm."CourseId" = le."CourseId" AND lm."IsActive" = TRUE)             AS "TotalModules"
    FROM "LMSEnrollments" le
    INNER JOIN "LMSCourses" lc ON le."CourseId" = lc."CourseId"
    INNER JOIN "Staff" s       ON le."StaffId"  = s."StaffId"
    WHERE le."TenantId" = p_tenant_id
      AND (p_staff_id  IS NULL OR le."StaffId"  = p_staff_id)
      AND (p_course_id IS NULL OR le."CourseId" = p_course_id)
      AND (p_status    IS NULL OR le."Status"   = p_status)
    ORDER BY le."EnrollmentDate" DESC
    OFFSET v_offset
    LIMIT  p_page_size;
END;
$$;

-- =============================================
-- Function: usp_GetLMSRecommendations
-- Uses CTE instead of temp table (#EmployeeSkills)
-- =============================================
DROP FUNCTION IF EXISTS usp_GetLMSRecommendations(INT, INT);

CREATE OR REPLACE FUNCTION usp_GetLMSRecommendations(
    p_tenant_id INT,
    p_staff_id  INT
)
RETURNS TABLE (
    "CourseId"             INT,
    "Title"                VARCHAR,
    "Description"          TEXT,
    "DurationHours"        INT,
    "DifficultyLevel"      VARCHAR,
    "SkillName"            VARCHAR,
    "RecommendationReason" VARCHAR,
    "RecommendationScore"  INT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_position   VARCHAR(100);
    v_department VARCHAR(100);
BEGIN
    SELECT s."Position", s."Department"
    INTO   v_position, v_department
    FROM   "Staff" s
    WHERE  s."StaffId" = p_staff_id AND s."TenantId" = p_tenant_id;

    RETURN QUERY
    WITH employee_skills AS (
        SELECT es."SkillId", sk."Name" AS "SkillName", es."ProficiencyLevel"
        FROM   "LMSEmployeeSkills" es
        INNER JOIN "LMSSkills" sk ON es."SkillId" = sk."SkillId"
        WHERE  es."StaffId" = p_staff_id
          AND  es."TenantId" = p_tenant_id
          AND  es."IsActive" = TRUE
    )
    SELECT DISTINCT
        lc."CourseId",
        lc."Title",
        lc."Description",
        lc."DurationHours",
        lc."DifficultyLevel",
        ls."Name"  AS "SkillName",
        CASE
            WHEN lc."SkillId" IN (
                SELECT es2."SkillId" FROM employee_skills es2
                WHERE es2."ProficiencyLevel" IN ('Beginner','Intermediate')
            ) THEN 'Skill Enhancement'
            WHEN lc."Title"       ILIKE '%' || v_position   || '%' THEN 'Position-based'
            WHEN lc."Description" ILIKE '%' || v_department || '%' THEN 'Department-based'
            ELSE 'General Recommendation'
        END::VARCHAR AS "RecommendationReason",
        CASE
            WHEN lc."SkillId" IN (
                SELECT es3."SkillId" FROM employee_skills es3
                WHERE es3."ProficiencyLevel" IN ('Beginner','Intermediate')
            ) THEN 90
            WHEN lc."Title"       ILIKE '%' || v_position   || '%' THEN 80
            WHEN lc."Description" ILIKE '%' || v_department || '%' THEN 70
            ELSE 50
        END AS "RecommendationScore"
    FROM "LMSCourses" lc
    LEFT JOIN "LMSSkills" ls ON lc."SkillId" = ls."SkillId"
    WHERE lc."TenantId" = p_tenant_id
      AND lc."IsActive" = TRUE
      AND lc."CourseId" NOT IN (
          SELECT le."CourseId" FROM "LMSEnrollments" le
          WHERE le."StaffId" = p_staff_id
            AND le."Status" IN ('Enrolled','InProgress','Completed')
      )
    ORDER BY "RecommendationScore" DESC, lc."CreatedDate" DESC;
END;
$$;
