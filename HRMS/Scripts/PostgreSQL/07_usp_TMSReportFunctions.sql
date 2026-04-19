-- =============================================
-- PostgreSQL Functions: TMS Reports (v2)
-- General Report | Trainer KPI | Statistics
-- =============================================

-- ─── GENERAL REPORT ──────────────────────────────────────────────────────────

DROP FUNCTION IF EXISTS usp_GetTMSGeneralReport(INT, DATE, DATE, INT, INT, VARCHAR, VARCHAR, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetTMSGeneralReport(
    p_tenant_id   INT,
    p_from_date   DATE    DEFAULT NULL,
    p_to_date     DATE    DEFAULT NULL,
    p_course_id   INT     DEFAULT NULL,
    p_trainer_id  INT     DEFAULT NULL,
    p_department  VARCHAR DEFAULT NULL,
    p_company     VARCHAR DEFAULT NULL,
    p_page_number INT     DEFAULT 1,
    p_page_size   INT     DEFAULT 20
)
RETURNS TABLE (
    "TotalCount"          BIGINT,
    "TotalClasses"        BIGINT,
    "GrandStaffAttended"  BIGINT,
    "GrandTotalHours"     NUMERIC(12,2),
    "RowNo"               BIGINT,
    "CoursePlanId"        INT,
    "StartDate"           DATE,
    "EndDate"             DATE,
    "StartTime"           TIME,
    "EndTime"             TIME,
    "Title"               VARCHAR,
    "CourseCode"          VARCHAR,
    "CourseType"          VARCHAR,
    "TotalHours"          NUMERIC(8,2),
    "TrainerName"         VARCHAR,
    "Venue"               VARCHAR,
    "TotalStaffAttended"  BIGINT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    WITH base_data AS (
        SELECT
            cp."Id"::INT                                                               AS plan_id,
            cp."StartDate"::DATE                                                       AS start_date,
            cp."EndDate"::DATE                                                         AS end_date,
            cp."StartTime"::TIME                                                       AS start_time,
            cp."EndTime"::TIME                                                         AS end_time,
            cr."Title"::VARCHAR                                                        AS title,
            cr."CourseCode"::VARCHAR                                                   AS course_code,
            COALESCE(md."Name", '')::VARCHAR                                           AS course_type,
            ROUND(EXTRACT(EPOCH FROM (cp."EndTime" - cp."StartTime")) / 3600.0, 2)::NUMERIC(8,2) AS total_hours,
            s."Name"::VARCHAR                                                          AS trainer_name,
            cp."Venue"::VARCHAR                                                        AS venue,
            (
                SELECT COUNT(*)::BIGINT FROM "CourseAttendance_DateWise" ca
                WHERE ca."CoursePlanId" = cp."Id"
                  AND ca."TenantId"     = p_tenant_id
                  AND ca."IsActive"     = TRUE
                  AND ca."IsPresent"    = TRUE
            )                                                                          AS staff_attended
        FROM "CoursePlanning" cp
        INNER JOIN "CourseRegistration"  cr ON cp."CourseId"  = cr."CourseId"
        INNER JOIN "Staff"               s  ON cp."TrainerId" = s."StaffId"
        LEFT  JOIN "tbl_Master_Dropdown" md ON md."Id"        = cr."CourseTypeId"
        WHERE cp."TenantId"  = p_tenant_id
          AND cp."IsActive"  = TRUE
          AND cr."IsActive"  = TRUE
          AND s."IsActive"   = TRUE
          AND (p_from_date  IS NULL OR cp."StartDate"::DATE >= p_from_date)
          AND (p_to_date    IS NULL OR cp."EndDate"::DATE   <= p_to_date)
          AND (p_course_id  IS NULL OR cp."CourseId"        = p_course_id)
          AND (p_trainer_id IS NULL OR cp."TrainerId"       = p_trainer_id)
          AND (p_department IS NULL OR LOWER(s."Department") = LOWER(p_department))
          AND (p_company    IS NULL OR LOWER(COALESCE(s."Company",'')) = LOWER(p_company))
    ),
    aggregates AS (
        SELECT
            COUNT(*)::BIGINT                       AS total_count,
            COUNT(*)::BIGINT                       AS total_classes,
            COALESCE(SUM(staff_attended),0)::BIGINT AS grand_staff_attended,
            COALESCE(SUM(total_hours),0)::NUMERIC(12,2) AS grand_total_hours
        FROM base_data
    ),
    numbered AS (
        SELECT *,
               ROW_NUMBER() OVER (ORDER BY start_date DESC, plan_id DESC) AS rn
        FROM base_data
    )
    SELECT
        a.total_count,
        a.total_classes,
        a.grand_staff_attended,
        a.grand_total_hours,
        n.rn::BIGINT,
        n.plan_id::INT,
        n.start_date,
        n.end_date,
        n.start_time,
        n.end_time,
        n.title,
        n.course_code,
        n.course_type,
        n.total_hours,
        n.trainer_name,
        n.venue,
        n.staff_attended::BIGINT
    FROM numbered n, aggregates a
    WHERE n.rn > ((p_page_number - 1) * p_page_size)
      AND n.rn <= (p_page_number * p_page_size)
    ORDER BY n.rn;
END;
$$;


-- ─── TRAINER KPI REPORT ──────────────────────────────────────────────────────

DROP FUNCTION IF EXISTS usp_GetTMSTrainerKPIReport(INT, INT, INT, DATE, DATE, INT, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetTMSTrainerKPIReport(
    p_tenant_id   INT,
    p_year        INT  DEFAULT NULL,
    p_month       INT  DEFAULT NULL,
    p_from_date   DATE DEFAULT NULL,
    p_to_date     DATE DEFAULT NULL,
    p_trainer_id  INT  DEFAULT NULL,
    p_page_number INT  DEFAULT 1,
    p_page_size   INT  DEFAULT 20
)
RETURNS TABLE (
    "TotalCount"   BIGINT,
    "GrandClasses" BIGINT,
    "GrandHours"   NUMERIC(12,2),
    "RowNo"        BIGINT,
    "StaffId"      INT,
    "TrainerName"  VARCHAR,
    "EmployeeCode" VARCHAR,
    "Department"   VARCHAR,
    "Year"         INT,
    "Month"        INT,
    "MonthName"    VARCHAR,
    "NumClasses"   BIGINT,
    "TotalHours"   NUMERIC(10,2)
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    WITH base_data AS (
        SELECT
            s."StaffId"::INT,
            s."Name"::VARCHAR                                                            AS trainer_name,
            s."EmployeeCode"::VARCHAR,
            s."Department"::VARCHAR,
            EXTRACT(YEAR  FROM cp."StartDate")::INT                                      AS yr,
            EXTRACT(MONTH FROM cp."StartDate")::INT                                      AS mo,
            TO_CHAR(cp."StartDate", 'Mon YYYY')::VARCHAR                                 AS month_name,
            COUNT(DISTINCT cp."Id")::BIGINT                                              AS num_classes,
            COALESCE(ROUND(SUM(
                EXTRACT(EPOCH FROM (cp."EndTime" - cp."StartTime")) / 3600.0
            ), 2), 0)::NUMERIC(10,2)                                                     AS total_hours
        FROM "CoursePlanning" cp
        INNER JOIN "Staff" s ON cp."TrainerId" = s."StaffId"
        WHERE cp."TenantId" = p_tenant_id
          AND cp."IsActive" = TRUE
          AND s."IsActive"  = TRUE
          AND (p_year       IS NULL OR EXTRACT(YEAR  FROM cp."StartDate") = p_year)
          AND (p_month      IS NULL OR EXTRACT(MONTH FROM cp."StartDate") = p_month)
          AND (p_from_date  IS NULL OR cp."StartDate"::DATE >= p_from_date)
          AND (p_to_date    IS NULL OR cp."EndDate"::DATE   <= p_to_date)
          AND (p_trainer_id IS NULL OR cp."TrainerId"       = p_trainer_id)
        GROUP BY s."StaffId", s."Name", s."EmployeeCode", s."Department",
                 EXTRACT(YEAR  FROM cp."StartDate"),
                 EXTRACT(MONTH FROM cp."StartDate"),
                 TO_CHAR(cp."StartDate", 'Mon YYYY')
    ),
    aggregates AS (
        SELECT
            COUNT(*)::BIGINT                          AS total_count,
            COALESCE(SUM(num_classes),0)::BIGINT      AS grand_classes,
            COALESCE(SUM(total_hours),0)::NUMERIC(12,2) AS grand_hours
        FROM base_data
    ),
    numbered AS (
        SELECT *, ROW_NUMBER() OVER (ORDER BY yr DESC, mo DESC, total_hours DESC) AS rn
        FROM base_data
    )
    SELECT
        a.total_count,
        a.grand_classes,
        a.grand_hours,
        n.rn::BIGINT,
        n."StaffId"::INT,
        n.trainer_name,
        n."EmployeeCode",
        n."Department",
        n.yr::INT,
        n.mo::INT,
        n.month_name,
        n.num_classes::BIGINT,
        n.total_hours
    FROM numbered n, aggregates a
    WHERE n.rn > ((p_page_number - 1) * p_page_size)
      AND n.rn <= (p_page_number * p_page_size)
    ORDER BY n.rn;
END;
$$;


-- ─── STATISTICS REPORT ───────────────────────────────────────────────────────

DROP FUNCTION IF EXISTS usp_GetTMSStatisticsReport(INT, VARCHAR, VARCHAR, INT, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetTMSStatisticsReport(
    p_tenant_id   INT,
    p_department  VARCHAR DEFAULT NULL,
    p_company     VARCHAR DEFAULT NULL,
    p_year        INT     DEFAULT NULL,
    p_page_number INT     DEFAULT 1,
    p_page_size   INT     DEFAULT 50
)
RETURNS TABLE (
    "TotalCount"         BIGINT,
    "GrandSessions"      BIGINT,
    "GrandEnrolled"      BIGINT,
    "GrandPresent"       BIGINT,
    "GrandAttendancePct" NUMERIC(5,2),
    "RowNo"              BIGINT,
    "Department"         VARCHAR,
    "CourseType"         VARCHAR,
    "TotalSessions"      BIGINT,
    "TotalEnrolled"      BIGINT,
    "TotalPresent"       BIGINT,
    "AttendancePct"      NUMERIC(5,2)
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    WITH base_data AS (
        SELECT
            COALESCE(s."Department", 'Unassigned')::VARCHAR                            AS department,
            COALESCE(md."Name", 'Unknown')::VARCHAR                                    AS course_type,
            COUNT(DISTINCT cp."Id")::BIGINT                                            AS total_sessions,
            COUNT(DISTINCT p2."CourseParticipantId")::BIGINT                           AS total_enrolled,
            COUNT(DISTINCT ca."AttendanceId") FILTER (WHERE ca."IsPresent" = TRUE)::BIGINT AS total_present
        FROM "CoursePlanning" cp
        INNER JOIN "CourseRegistration"  cr ON cp."CourseId" = cr."CourseId"
        LEFT  JOIN "tbl_Master_Dropdown" md ON md."Id" = cr."CourseTypeId"
        LEFT  JOIN "CourseParticipant"   p2 ON p2."CoursePlanId" = cp."Id"
                                            AND p2."TenantId"    = p_tenant_id
                                            AND p2."IsActive"    = TRUE
        LEFT  JOIN "Staff"               s  ON p2."StaffId" = s."StaffId"
                                            AND s."IsActive" = TRUE
        LEFT  JOIN "CourseAttendance_DateWise" ca ON ca."CoursePlanId" = cp."Id"
                                                  AND ca."StaffId"    = p2."StaffId"
                                                  AND ca."TenantId"   = p_tenant_id
                                                  AND ca."IsActive"   = TRUE
        WHERE cp."TenantId" = p_tenant_id
          AND cp."IsActive" = TRUE
          AND cr."IsActive" = TRUE
          AND (p_year       IS NULL OR EXTRACT(YEAR FROM cp."StartDate") = p_year)
          AND (p_department IS NULL OR LOWER(COALESCE(s."Department", '')) = LOWER(p_department))
          AND (p_company    IS NULL OR LOWER(COALESCE(s."Company",    '')) = LOWER(p_company))
        GROUP BY COALESCE(s."Department", 'Unassigned'), COALESCE(md."Name", 'Unknown')
    ),
    with_pct AS (
        SELECT *,
               CASE WHEN total_enrolled > 0
                    THEN ROUND(total_present * 100.0 / total_enrolled, 2)::NUMERIC(5,2)
                    ELSE 0::NUMERIC(5,2) END AS attendance_pct
        FROM base_data
    ),
    aggregates AS (
        SELECT
            COUNT(*)::BIGINT                          AS total_count,
            COALESCE(SUM(total_sessions),0)::BIGINT   AS grand_sessions,
            COALESCE(SUM(total_enrolled),0)::BIGINT   AS grand_enrolled,
            COALESCE(SUM(total_present),0)::BIGINT    AS grand_present,
            CASE WHEN SUM(total_enrolled) > 0
                 THEN ROUND(SUM(total_present) * 100.0 / SUM(total_enrolled), 2)::NUMERIC(5,2)
                 ELSE 0::NUMERIC(5,2) END             AS grand_pct
        FROM with_pct
    ),
    numbered AS (
        SELECT *, ROW_NUMBER() OVER (ORDER BY department ASC, total_sessions DESC) AS rn
        FROM with_pct
    )
    SELECT
        a.total_count,
        a.grand_sessions,
        a.grand_enrolled,
        a.grand_present,
        a.grand_pct,
        n.rn::BIGINT,
        n.department,
        n.course_type,
        n.total_sessions::BIGINT,
        n.total_enrolled::BIGINT,
        n.total_present::BIGINT,
        n.attendance_pct
    FROM numbered n, aggregates a
    WHERE n.rn > ((p_page_number - 1) * p_page_size)
      AND n.rn <= (p_page_number * p_page_size)
    ORDER BY n.rn;
END;
$$;
