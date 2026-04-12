-- =============================================
-- PostgreSQL Functions: Course Planning, Participants, Results
-- =============================================

-- =============================================
-- Function: usp_GetCourseParticipants
-- =============================================
DROP FUNCTION IF EXISTS usp_GetCourseParticipants(INT, INT);

CREATE OR REPLACE FUNCTION usp_GetCourseParticipants(
    p_course_plan_id INT,
    p_tenant_id      INT
)
RETURNS TABLE (
    "CourseParticipantId" INT,
    "CoursePlanId"        INT,
    "StaffId"             INT,
    "StaffName"           VARCHAR,
    "EmployeeCode"        VARCHAR,
    "Department"          VARCHAR,
    "Position"            VARCHAR,
    "Email"               VARCHAR,
    "PhoneNumber"         VARCHAR,
    "CreatedDate"         TIMESTAMP
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        cp."CourseParticipantId",
        cp."CoursePlanId",
        cp."StaffId",
        s."Name"          AS "StaffName",
        s."EmployeeCode",
        s."Department",
        s."Position",
        s."Email",
        s."PhoneNumber",
        cp."CreatedDate"
    FROM "CourseParticipant" cp
    INNER JOIN "Staff" s ON cp."StaffId" = s."StaffId"
    WHERE cp."CoursePlanId" = p_course_plan_id
      AND cp."TenantId"     = p_tenant_id
      AND cp."IsActive"     = TRUE
      AND s."IsActive"      = TRUE
    ORDER BY s."Name" ASC;
END;
$$;

-- =============================================
-- Function: usp_GetCourseResultSummary
-- =============================================
DROP FUNCTION IF EXISTS usp_GetCourseResultSummary(INT, INT);

CREATE OR REPLACE FUNCTION usp_GetCourseResultSummary(
    p_course_plan_id INT,
    p_tenant_id      INT
)
RETURNS TABLE (
    "ResultId"              INT,
    "CoursePlanId"          INT,
    "StaffId"               INT,
    "StaffName"             VARCHAR,
    "EmployeeCode"          VARCHAR,
    "Department"            VARCHAR,
    "Position"              VARCHAR,
    "TotalDays"             INT,
    "PresentDays"           INT,
    "AttendancePercentage"  NUMERIC(5,2),
    "Marks"                 NUMERIC(5,2),
    "ResultStatus"          VARCHAR,
    "CertificatePath"       VARCHAR,
    "CertificateSerialNumber" VARCHAR,
    "UpdatedDate"           TIMESTAMP
)
LANGUAGE plpgsql AS $$
DECLARE
    v_start_date DATE;
    v_end_date   DATE;
    v_total_days INT;
BEGIN
    SELECT cp."StartDate"::DATE, cp."EndDate"::DATE
    INTO   v_start_date, v_end_date
    FROM   "CoursePlanning" cp
    WHERE  cp."Id" = p_course_plan_id AND cp."TenantId" = p_tenant_id;

    v_total_days := COALESCE((v_end_date - v_start_date) + 1, 0);

    RETURN QUERY
    SELECT
        COALESCE(cr."ResultId", 0)                   AS "ResultId",
        cp2."CoursePlanId",
        cp2."StaffId",
        s."Name"                                     AS "StaffName",
        s."EmployeeCode",
        s."Department",
        s."Position",
        COALESCE(cr."TotalDays", v_total_days)        AS "TotalDays",
        COALESCE(cr."PresentDays", 0)                 AS "PresentDays",
        COALESCE(cr."AttendancePercentage", 0)        AS "AttendancePercentage",
        cr."Marks",
        cr."ResultStatus",
        cr."CertificatePath",
        cr."CertificateSerialNumber",
        cr."UpdatedDate"
    FROM "CourseParticipant" cp2
    INNER JOIN "Staff" s ON cp2."StaffId" = s."StaffId"
    LEFT JOIN  "CourseResult" cr ON
        cr."CoursePlanId" = cp2."CoursePlanId"
        AND cr."StaffId"  = cp2."StaffId"
        AND cr."TenantId" = p_tenant_id
        AND cr."IsActive" = TRUE
    WHERE cp2."CoursePlanId" = p_course_plan_id
      AND cp2."TenantId"     = p_tenant_id
      AND cp2."IsActive"     = TRUE
      AND s."IsActive"       = TRUE
    ORDER BY s."Name" ASC;
END;
$$;

-- =============================================
-- Function: usp_EnsureParticipantResult
-- =============================================
DROP FUNCTION IF EXISTS usp_EnsureParticipantResult(INT, INT, INT, INT);

CREATE OR REPLACE FUNCTION usp_EnsureParticipantResult(
    p_course_plan_id INT,
    p_staff_id       INT,
    p_tenant_id      INT,
    p_created_by     INT DEFAULT NULL
)
RETURNS VOID
LANGUAGE plpgsql AS $$
DECLARE
    v_total_days INT;
BEGIN
    SELECT (cp."EndDate"::DATE - cp."StartDate"::DATE) + 1
    INTO   v_total_days
    FROM   "CoursePlanning" cp
    WHERE  cp."Id" = p_course_plan_id AND cp."TenantId" = p_tenant_id;

    INSERT INTO "CourseResult" (
        "CoursePlanId", "StaffId", "TotalDays", "PresentDays", "AttendancePercentage",
        "TenantId", "IsActive", "CreatedDate", "CreatedBy"
    )
    SELECT
        p_course_plan_id, p_staff_id, v_total_days, 0, 0,
        p_tenant_id, TRUE, NOW(), p_created_by
    WHERE NOT EXISTS (
        SELECT 1 FROM "CourseResult"
        WHERE "CoursePlanId" = p_course_plan_id
          AND "StaffId"      = p_staff_id
          AND "TenantId"     = p_tenant_id
          AND "IsActive"     = TRUE
    );
END;
$$;
