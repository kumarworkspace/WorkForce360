-- =============================================
-- PostgreSQL Functions: TMS Reports
-- =============================================

-- =============================================
-- Function: usp_GetTMSOverallSummary
-- Returns a single-row KPI summary for TMS
-- =============================================
DROP FUNCTION IF EXISTS usp_GetTMSOverallSummary(INT, DATE, DATE, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetTMSOverallSummary(
    p_tenant_id   INT,
    p_from_date   DATE    DEFAULT NULL,
    p_to_date     DATE    DEFAULT NULL,
    p_trainer_id  INT     DEFAULT NULL,
    p_course_id   INT     DEFAULT NULL
)
RETURNS TABLE (
    "TotalCourses"              INT,
    "TotalSessions"             INT,
    "TotalParticipantsEnrolled" INT,
    "TotalPresent"              INT,
    "AvgAttendancePercentage"   NUMERIC(5,2),
    "CoursesCompleted"          INT,
    "CoursesOngoing"            INT,
    "CoursesUpcoming"           INT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_today DATE := CURRENT_DATE;
BEGIN
    RETURN QUERY
    SELECT
        COUNT(DISTINCT cr."CourseId")::INT                                              AS "TotalCourses",
        COUNT(DISTINCT cp."Id")::INT                                                    AS "TotalSessions",
        COALESCE(SUM(enrolled.cnt), 0)::INT                                             AS "TotalParticipantsEnrolled",
        COALESCE(SUM(present.cnt),  0)::INT                                             AS "TotalPresent",
        CASE
            WHEN COALESCE(SUM(enrolled.cnt), 0) > 0
            THEN CAST(COALESCE(SUM(present.cnt), 0) * 100.0 / SUM(enrolled.cnt) AS NUMERIC(5,2))
            ELSE 0
        END                                                                             AS "AvgAttendancePercentage",
        COUNT(DISTINCT CASE WHEN cp."EndDate"::DATE   <  v_today THEN cp."Id" END)::INT AS "CoursesCompleted",
        COUNT(DISTINCT CASE WHEN cp."StartDate"::DATE <= v_today
                             AND cp."EndDate"::DATE   >= v_today THEN cp."Id" END)::INT AS "CoursesOngoing",
        COUNT(DISTINCT CASE WHEN cp."StartDate"::DATE >  v_today THEN cp."Id" END)::INT AS "CoursesUpcoming"
    FROM "CoursePlanning" cp
    INNER JOIN "CourseRegistration" cr ON cp."CourseId" = cr."CourseId"
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS cnt
        FROM "CourseParticipant" p2
        WHERE p2."CoursePlanId" = cp."Id"
          AND p2."TenantId"     = p_tenant_id
          AND p2."IsActive"     = TRUE
    ) enrolled ON TRUE
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS cnt
        FROM "CourseAttendance_DateWise" ca
        WHERE ca."CoursePlanId" = cp."Id"
          AND ca."TenantId"     = p_tenant_id
          AND ca."IsActive"     = TRUE
          AND ca."IsPresent"    = TRUE
    ) present ON TRUE
    WHERE cp."TenantId"  = p_tenant_id
      AND cp."IsActive"  = TRUE
      AND (p_from_date   IS NULL OR cp."StartDate"::DATE >= p_from_date)
      AND (p_to_date     IS NULL OR cp."EndDate"::DATE   <= p_to_date)
      AND (p_trainer_id  IS NULL OR cp."TrainerId"       = p_trainer_id)
      AND (p_course_id   IS NULL OR cp."CourseId"        = p_course_id);
END;
$$;

-- =============================================
-- Function: usp_GetTMSMonthlySummary
-- Returns month-by-month course/attendance data
-- =============================================
DROP FUNCTION IF EXISTS usp_GetTMSMonthlySummary(INT, DATE, DATE, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetTMSMonthlySummary(
    p_tenant_id   INT,
    p_from_date   DATE DEFAULT NULL,
    p_to_date     DATE DEFAULT NULL,
    p_trainer_id  INT  DEFAULT NULL,
    p_course_id   INT  DEFAULT NULL
)
RETURNS TABLE (
    "Year"                   INT,
    "Month"                  INT,
    "MonthName"              VARCHAR,
    "TotalSessions"          INT,
    "TotalParticipants"      INT,
    "TotalPresent"           INT,
    "AvgAttendancePercentage" NUMERIC(5,2)
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        EXTRACT(YEAR  FROM cp."StartDate")::INT                                           AS "Year",
        EXTRACT(MONTH FROM cp."StartDate")::INT                                           AS "Month",
        TO_CHAR(cp."StartDate", 'Mon YYYY')::VARCHAR                                      AS "MonthName",
        COUNT(DISTINCT cp."Id")::INT                                                       AS "TotalSessions",
        COALESCE(SUM(enrolled.cnt), 0)::INT                                                AS "TotalParticipants",
        COALESCE(SUM(present.cnt),  0)::INT                                                AS "TotalPresent",
        CASE
            WHEN COALESCE(SUM(enrolled.cnt), 0) > 0
            THEN CAST(COALESCE(SUM(present.cnt), 0) * 100.0 / SUM(enrolled.cnt) AS NUMERIC(5,2))
            ELSE 0
        END                                                                                AS "AvgAttendancePercentage"
    FROM "CoursePlanning" cp
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS cnt
        FROM "CourseParticipant" p2
        WHERE p2."CoursePlanId" = cp."Id"
          AND p2."TenantId"     = p_tenant_id
          AND p2."IsActive"     = TRUE
    ) enrolled ON TRUE
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS cnt
        FROM "CourseAttendance_DateWise" ca
        WHERE ca."CoursePlanId" = cp."Id"
          AND ca."TenantId"     = p_tenant_id
          AND ca."IsActive"     = TRUE
          AND ca."IsPresent"    = TRUE
    ) present ON TRUE
    WHERE cp."TenantId"  = p_tenant_id
      AND cp."IsActive"  = TRUE
      AND (p_from_date   IS NULL OR cp."StartDate"::DATE >= p_from_date)
      AND (p_to_date     IS NULL OR cp."EndDate"::DATE   <= p_to_date)
      AND (p_trainer_id  IS NULL OR cp."TrainerId"       = p_trainer_id)
      AND (p_course_id   IS NULL OR cp."CourseId"        = p_course_id)
    GROUP BY
        EXTRACT(YEAR  FROM cp."StartDate"),
        EXTRACT(MONTH FROM cp."StartDate"),
        TO_CHAR(cp."StartDate", 'Mon YYYY')
    ORDER BY "Year" DESC, "Month" DESC;
END;
$$;

-- =============================================
-- Function: usp_GetTMSTrainerPerformance
-- Returns per-trainer courses and attendance stats
-- =============================================
DROP FUNCTION IF EXISTS usp_GetTMSTrainerPerformance(INT, DATE, DATE, INT);

CREATE OR REPLACE FUNCTION usp_GetTMSTrainerPerformance(
    p_tenant_id   INT,
    p_from_date   DATE DEFAULT NULL,
    p_to_date     DATE DEFAULT NULL,
    p_trainer_id  INT  DEFAULT NULL
)
RETURNS TABLE (
    "StaffId"                  INT,
    "TrainerName"              VARCHAR,
    "EmployeeCode"             VARCHAR,
    "Department"               VARCHAR,
    "TotalCoursesConducted"    INT,
    "TotalParticipantsTrained" INT,
    "AvgAttendancePercentage"  NUMERIC(5,2)
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        s."StaffId",
        s."Name"::VARCHAR                                                                  AS "TrainerName",
        s."EmployeeCode",
        s."Department",
        COUNT(DISTINCT cp."Id")::INT                                                       AS "TotalCoursesConducted",
        COALESCE(SUM(enrolled.cnt), 0)::INT                                                AS "TotalParticipantsTrained",
        CASE
            WHEN COALESCE(SUM(enrolled.cnt), 0) > 0
            THEN CAST(COALESCE(SUM(present.cnt), 0) * 100.0 / SUM(enrolled.cnt) AS NUMERIC(5,2))
            ELSE 0
        END                                                                                AS "AvgAttendancePercentage"
    FROM "CoursePlanning" cp
    INNER JOIN "Staff" s ON cp."TrainerId" = s."StaffId"
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS cnt
        FROM "CourseParticipant" p2
        WHERE p2."CoursePlanId" = cp."Id"
          AND p2."TenantId"     = p_tenant_id
          AND p2."IsActive"     = TRUE
    ) enrolled ON TRUE
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS cnt
        FROM "CourseAttendance_DateWise" ca
        WHERE ca."CoursePlanId" = cp."Id"
          AND ca."TenantId"     = p_tenant_id
          AND ca."IsActive"     = TRUE
          AND ca."IsPresent"    = TRUE
    ) present ON TRUE
    WHERE cp."TenantId"  = p_tenant_id
      AND cp."IsActive"  = TRUE
      AND (p_from_date   IS NULL OR cp."StartDate"::DATE >= p_from_date)
      AND (p_to_date     IS NULL OR cp."EndDate"::DATE   <= p_to_date)
      AND (p_trainer_id  IS NULL OR cp."TrainerId"       = p_trainer_id)
    GROUP BY s."StaffId", s."Name", s."EmployeeCode", s."Department"
    ORDER BY "TotalCoursesConducted" DESC, s."Name" ASC;
END;
$$;

-- =============================================
-- Function: usp_GetTMSCourseWiseReport
-- Returns per-course attendance and enrollment stats
-- =============================================
DROP FUNCTION IF EXISTS usp_GetTMSCourseWiseReport(INT, DATE, DATE, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetTMSCourseWiseReport(
    p_tenant_id   INT,
    p_from_date   DATE DEFAULT NULL,
    p_to_date     DATE DEFAULT NULL,
    p_trainer_id  INT  DEFAULT NULL,
    p_course_id   INT  DEFAULT NULL
)
RETURNS TABLE (
    "CourseId"                 INT,
    "CourseTitle"              VARCHAR,
    "CourseCode"               VARCHAR,
    "Category"                 VARCHAR,
    "TotalSessions"            INT,
    "TotalParticipantsEnrolled" INT,
    "TotalPresent"             INT,
    "AvgAttendancePercentage"  NUMERIC(5,2)
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        cr."CourseId",
        cr."Title"::VARCHAR                                                                AS "CourseTitle",
        cr."CourseCode",
        cr."Category",
        COUNT(DISTINCT cp."Id")::INT                                                       AS "TotalSessions",
        COALESCE(SUM(enrolled.cnt), 0)::INT                                                AS "TotalParticipantsEnrolled",
        COALESCE(SUM(present.cnt),  0)::INT                                                AS "TotalPresent",
        CASE
            WHEN COALESCE(SUM(enrolled.cnt), 0) > 0
            THEN CAST(COALESCE(SUM(present.cnt), 0) * 100.0 / SUM(enrolled.cnt) AS NUMERIC(5,2))
            ELSE 0
        END                                                                                AS "AvgAttendancePercentage"
    FROM "CourseRegistration" cr
    INNER JOIN "CoursePlanning" cp ON cp."CourseId" = cr."CourseId"
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS cnt
        FROM "CourseParticipant" p2
        WHERE p2."CoursePlanId" = cp."Id"
          AND p2."TenantId"     = p_tenant_id
          AND p2."IsActive"     = TRUE
    ) enrolled ON TRUE
    LEFT JOIN LATERAL (
        SELECT COUNT(*) AS cnt
        FROM "CourseAttendance_DateWise" ca
        WHERE ca."CoursePlanId" = cp."Id"
          AND ca."TenantId"     = p_tenant_id
          AND ca."IsActive"     = TRUE
          AND ca."IsPresent"    = TRUE
    ) present ON TRUE
    WHERE cp."TenantId"  = p_tenant_id
      AND cp."IsActive"  = TRUE
      AND (p_from_date   IS NULL OR cp."StartDate"::DATE >= p_from_date)
      AND (p_to_date     IS NULL OR cp."EndDate"::DATE   <= p_to_date)
      AND (p_trainer_id  IS NULL OR cp."TrainerId"       = p_trainer_id)
      AND (p_course_id   IS NULL OR cr."CourseId"        = p_course_id)
    GROUP BY cr."CourseId", cr."Title", cr."CourseCode", cr."Category"
    ORDER BY "TotalSessions" DESC, cr."Title" ASC;
END;
$$;
