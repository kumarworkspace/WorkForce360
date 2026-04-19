-- =============================================
-- PostgreSQL Functions: Course Attendance Date-Wise CRUD
-- =============================================

-- =============================================
-- Function: usp_GetCourseAttendanceDateWise
-- =============================================
DROP FUNCTION IF EXISTS usp_GetCourseAttendanceDateWise(INT, INT, DATE, INT);

CREATE OR REPLACE FUNCTION usp_GetCourseAttendanceDateWise(
    p_course_plan_id  INT,
    p_tenant_id       INT,
    p_attendance_date DATE DEFAULT NULL,
    p_staff_id        INT  DEFAULT NULL
)
RETURNS TABLE (
    "AttendanceId"    INT,
    "CoursePlanId"    INT,
    "StaffId"         INT,
    "StaffName"       VARCHAR,
    "EmployeeCode"    VARCHAR,
    "Department"      VARCHAR,
    "Division"        VARCHAR,
    "Position"        VARCHAR,
    "StaffPhoto"      VARCHAR,
    "AttendanceDate"  TIMESTAMP,
    "IsPresent"       BOOLEAN,
    "Remarks"         VARCHAR,
    "TenantId"        INT,
    "IsActive"        BOOLEAN,
    "CreatedDate"     TIMESTAMP,
    "CreatedBy"       INT,
    "UpdatedDate"     TIMESTAMP,
    "UpdatedBy"       INT,
    "CourseStartDate" TIMESTAMP,
    "CourseEndDate"   TIMESTAMP,
    "CourseTitle"     VARCHAR,
    "CourseNumber"    VARCHAR,
    "TrainerName"     VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        ca."AttendanceId",
        ca."CoursePlanId",
        ca."StaffId",
        s."Name"::VARCHAR,
        s."EmployeeCode",
        s."Department",
        s."Division",
        s."Position",
        s."Photo"::VARCHAR,
        ca."AttendanceDate"::TIMESTAMP,
        ca."IsPresent",
        ca."Remarks",
        ca."TenantId",
        ca."IsActive",
        ca."CreatedDate",
        ca."CreatedBy",
        ca."UpdatedDate",
        ca."UpdatedBy",
        cp."StartDate"::TIMESTAMP AS "CourseStartDate",
        cp."EndDate"::TIMESTAMP   AS "CourseEndDate",
        cr."Title"::VARCHAR       AS "CourseTitle",
        cr."CourseCode"::VARCHAR  AS "CourseNumber",
        trainer."Name"::VARCHAR   AS "TrainerName"
    FROM "CourseAttendance_DateWise" ca
    INNER JOIN "Staff" s             ON ca."StaffId"      = s."StaffId"
    INNER JOIN "CoursePlanning" cp   ON ca."CoursePlanId" = cp."Id"
    INNER JOIN "CourseRegistration" cr ON cp."CourseId"   = cr."CourseId"
    INNER JOIN "Staff" trainer       ON cp."TrainerId"    = trainer."StaffId"
    WHERE ca."CoursePlanId" = p_course_plan_id
      AND ca."TenantId"     = p_tenant_id
      AND ca."IsActive"     = TRUE
      AND (p_attendance_date IS NULL OR ca."AttendanceDate"::DATE = p_attendance_date)
      AND (p_staff_id        IS NULL OR ca."StaffId"              = p_staff_id)
    ORDER BY ca."AttendanceDate" DESC, s."Name" ASC;
END;
$$;

-- =============================================
-- Function: usp_CreateCourseAttendanceDateWise
-- =============================================
DROP FUNCTION IF EXISTS usp_CreateCourseAttendanceDateWise(INT, INT, DATE, BOOLEAN, VARCHAR, INT, INT);

CREATE OR REPLACE FUNCTION usp_CreateCourseAttendanceDateWise(
    p_course_plan_id  INT,
    p_staff_id        INT,
    p_attendance_date DATE,
    p_tenant_id       INT,
    p_is_present      BOOLEAN DEFAULT TRUE,
    p_remarks         VARCHAR(500) DEFAULT NULL,
    p_created_by      INT     DEFAULT NULL
)
RETURNS TABLE (
    "AttendanceId" INT,
    "Message"      VARCHAR,
    "Success"      BOOLEAN
)
LANGUAGE plpgsql AS $$
DECLARE
    v_attendance_id INT;
BEGIN
    -- Check if attendance already exists
    SELECT ca."AttendanceId" INTO v_attendance_id
    FROM "CourseAttendance_DateWise" ca
    WHERE ca."CoursePlanId"    = p_course_plan_id
      AND ca."StaffId"         = p_staff_id
      AND ca."AttendanceDate"::DATE = p_attendance_date
      AND ca."TenantId"        = p_tenant_id
      AND ca."IsActive"        = TRUE
    LIMIT 1;

    IF v_attendance_id IS NOT NULL THEN
        -- Update existing record
        UPDATE "CourseAttendance_DateWise"
        SET "IsPresent"   = p_is_present,
            "Remarks"     = p_remarks,
            "UpdatedDate" = NOW(),
            "UpdatedBy"   = p_created_by
        WHERE "AttendanceId" = v_attendance_id;

        RETURN QUERY SELECT v_attendance_id, 'Attendance updated successfully'::VARCHAR, TRUE;
    ELSE
        -- Insert new record
        INSERT INTO "CourseAttendance_DateWise" (
            "CoursePlanId", "StaffId", "AttendanceDate", "IsPresent", "Remarks",
            "TenantId", "IsActive", "CreatedDate", "CreatedBy"
        )
        VALUES (
            p_course_plan_id, p_staff_id, p_attendance_date, p_is_present, p_remarks,
            p_tenant_id, TRUE, NOW(), p_created_by
        )
        RETURNING "AttendanceId" INTO v_attendance_id;

        RETURN QUERY SELECT v_attendance_id, 'Attendance created successfully'::VARCHAR, TRUE;
    END IF;
END;
$$;

-- =============================================
-- Function: usp_UpdateCourseAttendanceDateWise
-- =============================================
DROP FUNCTION IF EXISTS usp_UpdateCourseAttendanceDateWise(INT, BOOLEAN, VARCHAR, INT, INT);

CREATE OR REPLACE FUNCTION usp_UpdateCourseAttendanceDateWise(
    p_attendance_id INT,
    p_is_present    BOOLEAN,
    p_tenant_id     INT,
    p_remarks       VARCHAR(500) DEFAULT NULL,
    p_updated_by    INT DEFAULT NULL
)
RETURNS TABLE (
    "Success" BOOLEAN,
    "Message" VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM "CourseAttendance_DateWise"
        WHERE "AttendanceId" = p_attendance_id
          AND "TenantId"     = p_tenant_id
          AND "IsActive"     = TRUE
    ) THEN
        RETURN QUERY SELECT FALSE, 'Attendance record not found'::VARCHAR;
        RETURN;
    END IF;

    UPDATE "CourseAttendance_DateWise"
    SET "IsPresent"   = p_is_present,
        "Remarks"     = p_remarks,
        "UpdatedDate" = NOW(),
        "UpdatedBy"   = p_updated_by
    WHERE "AttendanceId" = p_attendance_id
      AND "TenantId"     = p_tenant_id
      AND "IsActive"     = TRUE;

    RETURN QUERY SELECT TRUE, 'Attendance updated successfully'::VARCHAR;
END;
$$;

-- =============================================
-- Function: usp_DeleteCourseAttendanceDateWise
-- =============================================
DROP FUNCTION IF EXISTS usp_DeleteCourseAttendanceDateWise(INT, INT, INT);

CREATE OR REPLACE FUNCTION usp_DeleteCourseAttendanceDateWise(
    p_attendance_id INT,
    p_tenant_id     INT,
    p_updated_by    INT DEFAULT NULL
)
RETURNS TABLE (
    "Success" BOOLEAN,
    "Message" VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM "CourseAttendance_DateWise"
        WHERE "AttendanceId" = p_attendance_id
          AND "TenantId"     = p_tenant_id
          AND "IsActive"     = TRUE
    ) THEN
        RETURN QUERY SELECT FALSE, 'Attendance record not found'::VARCHAR;
        RETURN;
    END IF;

    UPDATE "CourseAttendance_DateWise"
    SET "IsActive"    = FALSE,
        "UpdatedDate" = NOW(),
        "UpdatedBy"   = p_updated_by
    WHERE "AttendanceId" = p_attendance_id
      AND "TenantId"     = p_tenant_id;

    RETURN QUERY SELECT TRUE, 'Attendance deleted successfully'::VARCHAR;
END;
$$;

-- =============================================
-- Function: usp_BulkMarkCourseAttendanceDateWise
-- Uses jsonb_array_elements to replace OPENJSON / CURSOR pattern
-- JSON format: [{"StaffId":1,"IsPresent":true,"Remarks":"..."},...]
-- =============================================
DROP FUNCTION IF EXISTS usp_BulkMarkCourseAttendanceDateWise(INT, DATE, INT, INT, TEXT);

CREATE OR REPLACE FUNCTION usp_BulkMarkCourseAttendanceDateWise(
    p_course_plan_id  INT,
    p_attendance_date DATE,
    p_tenant_id       INT,
    p_created_by      INT  DEFAULT NULL,
    p_attendance_data TEXT DEFAULT NULL
)
RETURNS TABLE (
    "Success" BOOLEAN,
    "Message" VARCHAR
)
LANGUAGE plpgsql AS $$
DECLARE
    rec       JSONB;
    v_staff   INT;
    v_present BOOLEAN;
    v_remarks VARCHAR(500);
BEGIN
    FOR rec IN
        SELECT elem
        FROM jsonb_array_elements(p_attendance_data::JSONB) AS elem
    LOOP
        v_staff   := (rec->>'StaffId')::INT;
        v_present := COALESCE((rec->>'IsPresent')::BOOLEAN, TRUE);
        v_remarks := rec->>'Remarks';

        IF EXISTS (
            SELECT 1 FROM "CourseAttendance_DateWise"
            WHERE "CoursePlanId"       = p_course_plan_id
              AND "StaffId"            = v_staff
              AND "AttendanceDate"::DATE = p_attendance_date
              AND "TenantId"           = p_tenant_id
              AND "IsActive"           = TRUE
        ) THEN
            UPDATE "CourseAttendance_DateWise"
            SET "IsPresent"   = v_present,
                "Remarks"     = v_remarks,
                "UpdatedDate" = NOW(),
                "UpdatedBy"   = p_created_by
            WHERE "CoursePlanId"         = p_course_plan_id
              AND "StaffId"              = v_staff
              AND "AttendanceDate"::DATE = p_attendance_date
              AND "TenantId"             = p_tenant_id
              AND "IsActive"             = TRUE;
        ELSE
            INSERT INTO "CourseAttendance_DateWise" (
                "CoursePlanId", "StaffId", "AttendanceDate", "IsPresent", "Remarks",
                "TenantId", "IsActive", "CreatedDate", "CreatedBy"
            )
            VALUES (
                p_course_plan_id, v_staff, p_attendance_date, v_present, v_remarks,
                p_tenant_id, TRUE, NOW(), p_created_by
            );
        END IF;
    END LOOP;

    RETURN QUERY SELECT TRUE, 'Bulk attendance saved successfully'::VARCHAR;

EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT FALSE, SQLERRM::VARCHAR;
END;
$$;

-- =============================================
-- Function: usp_GetAttendanceSummaryByCoursePlan_Staff
-- Staff-level attendance summary (first of two split functions)
-- =============================================
DROP FUNCTION IF EXISTS usp_GetAttendanceSummaryByCoursePlan_Staff(INT, INT);

CREATE OR REPLACE FUNCTION usp_GetAttendanceSummaryByCoursePlan_Staff(
    p_course_plan_id INT,
    p_tenant_id      INT
)
RETURNS TABLE (
    "StaffId"             INT,
    "StaffName"           VARCHAR,
    "EmployeeCode"        VARCHAR,
    "Department"          VARCHAR,
    "Division"            VARCHAR,
    "Position"            VARCHAR,
    "StaffPhoto"          VARCHAR,
    "TotalCourseDays"     INT,
    "DaysPresent"         INT,
    "DaysAbsent"          INT,
    "DaysMarked"          INT,
    "DaysNotMarked"       INT,
    "AttendancePercentage" NUMERIC(5,2)
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

    v_total_days := (v_end_date - v_start_date) + 1;

    RETURN QUERY
    SELECT
        s."StaffId",
        s."Name"                                                                           AS "StaffName",
        s."EmployeeCode",
        s."Department",
        s."Division",
        s."Position",
        s."Photo"                                                                          AS "StaffPhoto",
        v_total_days                                                                       AS "TotalCourseDays",
        COALESCE(SUM(CASE WHEN ca."IsPresent" = TRUE  THEN 1 ELSE 0 END), 0)::INT         AS "DaysPresent",
        COALESCE(SUM(CASE WHEN ca."IsPresent" = FALSE THEN 1 ELSE 0 END), 0)::INT         AS "DaysAbsent",
        COALESCE(COUNT(ca."AttendanceId"), 0)::INT                                         AS "DaysMarked",
        (v_total_days - COALESCE(COUNT(ca."AttendanceId"), 0))::INT                        AS "DaysNotMarked",
        CASE
            WHEN v_total_days > 0 THEN
                CAST(COALESCE(SUM(CASE WHEN ca."IsPresent" = TRUE THEN 1 ELSE 0 END), 0)
                     * 100.0 / v_total_days AS NUMERIC(5,2))
            ELSE 0
        END                                                                                AS "AttendancePercentage"
    FROM "CourseParticipant" cp2
    INNER JOIN "Staff" s ON cp2."StaffId" = s."StaffId"
    LEFT JOIN  "CourseAttendance_DateWise" ca ON
        ca."CoursePlanId" = cp2."CoursePlanId"
        AND ca."StaffId"  = cp2."StaffId"
        AND ca."TenantId" = p_tenant_id
        AND ca."IsActive" = TRUE
    WHERE cp2."CoursePlanId" = p_course_plan_id
      AND cp2."TenantId"     = p_tenant_id
      AND cp2."IsActive"     = TRUE
    GROUP BY s."StaffId", s."Name", s."EmployeeCode", s."Department", s."Division", s."Position", s."Photo"
    ORDER BY s."Name";
END;
$$;

-- =============================================
-- Function: usp_GetAttendanceSummaryByCoursePlan_Daily
-- Daily attendance summary (second of two split functions)
-- =============================================
DROP FUNCTION IF EXISTS usp_GetAttendanceSummaryByCoursePlan_Daily(INT, INT);

CREATE OR REPLACE FUNCTION usp_GetAttendanceSummaryByCoursePlan_Daily(
    p_course_plan_id INT,
    p_tenant_id      INT
)
RETURNS TABLE (
    "AttendanceDate"   TIMESTAMP,
    "TotalParticipants" INT,
    "PresentCount"     INT,
    "AbsentCount"      INT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        ca."AttendanceDate",
        COUNT(*)::INT                                                    AS "TotalParticipants",
        SUM(CASE WHEN ca."IsPresent" = TRUE  THEN 1 ELSE 0 END)::INT    AS "PresentCount",
        SUM(CASE WHEN ca."IsPresent" = FALSE THEN 1 ELSE 0 END)::INT    AS "AbsentCount"
    FROM "CourseAttendance_DateWise" ca
    WHERE ca."CoursePlanId" = p_course_plan_id
      AND ca."TenantId"     = p_tenant_id
      AND ca."IsActive"     = TRUE
    GROUP BY ca."AttendanceDate"
    ORDER BY ca."AttendanceDate";
END;
$$;

-- =============================================
-- Function: usp_GetAttendanceByCoursePlan
-- Retrieves attendance records for a course plan with staff details.
-- Used by the attendance grid for display and checkbox state.
-- =============================================
DROP FUNCTION IF EXISTS usp_GetAttendanceByCoursePlan(INT, INT);

CREATE OR REPLACE FUNCTION usp_GetAttendanceByCoursePlan(
    p_course_plan_id INT,
    p_tenant_id      INT
)
RETURNS TABLE (
    "AttendanceId"   INT,
    "CoursePlanId"   INT,
    "StaffId"        INT,
    "StaffName"      VARCHAR,
    "EmployeeCode"   VARCHAR,
    "Department"     VARCHAR,
    "Position"       VARCHAR,
    "AttendanceDate" TIMESTAMP,
    "IsPresent"      BOOLEAN,
    "Remarks"        VARCHAR,
    "TenantId"       INT,
    "IsActive"       BOOLEAN,
    "CreatedDate"    TIMESTAMP,
    "CreatedBy"      INT,
    "UpdatedDate"    TIMESTAMP,
    "UpdatedBy"      INT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        ca."AttendanceId",
        ca."CoursePlanId",
        ca."StaffId",
        s."Name"::VARCHAR          AS "StaffName",
        s."EmployeeCode",
        s."Department",
        s."Position",
        ca."AttendanceDate"::TIMESTAMP,
        ca."IsPresent",
        ca."Remarks",
        ca."TenantId",
        ca."IsActive",
        ca."CreatedDate"::TIMESTAMP,
        ca."CreatedBy",
        ca."UpdatedDate"::TIMESTAMP,
        ca."UpdatedBy"
    FROM "CourseAttendance_DateWise" ca
    INNER JOIN "Staff" s ON ca."StaffId" = s."StaffId"
    WHERE ca."CoursePlanId" = p_course_plan_id
      AND ca."TenantId"     = p_tenant_id
      AND ca."IsActive"     = TRUE
    ORDER BY ca."AttendanceDate" ASC, s."Name" ASC;
END;
$$;
