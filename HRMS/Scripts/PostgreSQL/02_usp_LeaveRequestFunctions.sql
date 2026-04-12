-- =============================================
-- PostgreSQL Functions: Leave Request stored procedures
-- =============================================

-- =============================================
-- Function: usp_GetLeaveRequestList
-- Returns single result set with TotalCount (COUNT(*) OVER())
-- =============================================
DROP FUNCTION IF EXISTS usp_GetLeaveRequestList(INT, INT, INT, INT, INT, DATE, DATE, VARCHAR, BOOLEAN, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetLeaveRequestList(
    p_tenant_id       INT,
    p_staff_id        INT      DEFAULT NULL,
    p_request_type_id INT      DEFAULT NULL,
    p_leave_type_id   INT      DEFAULT NULL,
    p_leave_status    INT      DEFAULT NULL,
    p_from_date       DATE     DEFAULT NULL,
    p_to_date         DATE     DEFAULT NULL,
    p_search_term     VARCHAR(200) DEFAULT NULL,
    p_is_active       BOOLEAN  DEFAULT TRUE,
    p_page_number     INT      DEFAULT 1,
    p_page_size       INT      DEFAULT 10
)
RETURNS TABLE (
    "TotalCount"            INT,
    "RequestId"             INT,
    "StaffId"               INT,
    "StaffName"             VARCHAR,
    "EmployeeCode"          VARCHAR,
    "Department"            VARCHAR,
    "Division"              VARCHAR,
    "Position"              VARCHAR,
    "RequestTypeId"         INT,
    "RequestTypeName"       VARCHAR,
    "LeaveTypeId"           INT,
    "LeaveTypeName"         VARCHAR,
    "FromDate"              TIMESTAMP,
    "ToDate"                TIMESTAMP,
    "TotalDays"             NUMERIC,
    "TotalHours"            NUMERIC,
    "Reason"                VARCHAR,
    "LeaveStatus"           INT,
    "LeaveStatusName"       VARCHAR,
    "ReportingManagerId"    INT,
    "ReportingManagerName"  VARCHAR,
    "ReportingManagerCode"  VARCHAR,
    "HRApprovalRequired"    BOOLEAN,
    "ApprovedBy_L1"         INT,
    "ApprovedByL1Name"      VARCHAR,
    "ApprovedDate_L1"       TIMESTAMP,
    "ApprovedBy_HR"         INT,
    "ApprovedByHRName"      VARCHAR,
    "ApprovedDate_HR"       TIMESTAMP,
    "Attachment"            VARCHAR,
    "IsActive"              BOOLEAN,
    "TenantId"              INT,
    "CreatedDate"           TIMESTAMP,
    "CreatedBy"             INT,
    "UpdatedDate"           TIMESTAMP,
    "UpdatedBy"             INT
)
LANGUAGE plpgsql AS $$
DECLARE
    v_offset INT := (p_page_number - 1) * p_page_size;
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) OVER()::INT AS "TotalCount",
        lr."RequestId",
        lr."StaffId",
        s."Name"              AS "StaffName",
        s."EmployeeCode",
        s."Department",
        s."Division",
        s."Position",
        lr."RequestTypeId",
        rt."Name"             AS "RequestTypeName",
        lr."LeaveTypeId",
        lt."LeaveTypeName",
        lr."FromDate",
        lr."ToDate",
        lr."TotalDays",
        lr."TotalHours",
        lr."Reason",
        lr."LeaveStatus",
        ls."Name"             AS "LeaveStatusName",
        lr."ReportingManagerId",
        rm."Name"             AS "ReportingManagerName",
        rm."EmployeeCode"     AS "ReportingManagerCode",
        lr."HRApprovalRequired",
        lr."ApprovedBy_L1",
        l1."Name"             AS "ApprovedByL1Name",
        lr."ApprovedDate_L1",
        lr."ApprovedBy_HR",
        hr."Name"             AS "ApprovedByHRName",
        lr."ApprovedDate_HR",
        lr."Attachment",
        lr."IsActive",
        lr."TenantId",
        lr."CreatedDate",
        lr."CreatedBy",
        lr."UpdatedDate",
        lr."UpdatedBy"
    FROM "Leave_OT_Request" lr
    INNER JOIN "Staff" s               ON lr."StaffId"            = s."StaffId"
    LEFT JOIN  "tbl_Master_Dropdown" rt ON lr."RequestTypeId"     = rt."Id"
    LEFT JOIN  "LeaveTypeMaster" lt     ON lr."LeaveTypeId"       = lt."LeaveTypeId"
    LEFT JOIN  "tbl_Master_Dropdown" ls ON lr."LeaveStatus"       = ls."Id"
    LEFT JOIN  "Staff" rm               ON lr."ReportingManagerId"= rm."StaffId"
    LEFT JOIN  "Staff" l1               ON lr."ApprovedBy_L1"     = l1."StaffId"
    LEFT JOIN  "Staff" hr               ON lr."ApprovedBy_HR"     = hr."StaffId"
    WHERE lr."TenantId" = p_tenant_id
      AND (p_is_active       IS NULL OR lr."IsActive"      = p_is_active)
      AND (p_staff_id        IS NULL OR lr."StaffId"       = p_staff_id)
      AND (p_request_type_id IS NULL OR lr."RequestTypeId" = p_request_type_id)
      AND (p_leave_type_id   IS NULL OR lr."LeaveTypeId"   = p_leave_type_id)
      AND (p_leave_status    IS NULL OR lr."LeaveStatus"   = p_leave_status)
      AND (p_from_date       IS NULL OR lr."FromDate"     >= p_from_date)
      AND (p_to_date         IS NULL OR lr."ToDate"       <= p_to_date)
      AND (p_search_term IS NULL OR p_search_term = '' OR
           s."Name"         ILIKE '%' || p_search_term || '%' OR
           s."EmployeeCode" ILIKE '%' || p_search_term || '%')
    ORDER BY lr."CreatedDate" DESC
    OFFSET v_offset
    LIMIT  p_page_size;
END;
$$;

-- =============================================
-- Function: usp_GetLeaveRequestByStaff
-- =============================================
DROP FUNCTION IF EXISTS usp_GetLeaveRequestByStaff(INT, INT, INT, INT, BOOLEAN);

CREATE OR REPLACE FUNCTION usp_GetLeaveRequestByStaff(
    p_tenant_id       INT,
    p_staff_id        INT,
    p_request_type_id INT     DEFAULT NULL,
    p_year            INT     DEFAULT NULL,
    p_is_active       BOOLEAN DEFAULT TRUE
)
RETURNS TABLE (
    "RequestId"             INT,
    "StaffId"               INT,
    "StaffName"             VARCHAR,
    "EmployeeCode"          VARCHAR,
    "RequestTypeId"         INT,
    "RequestTypeName"       VARCHAR,
    "LeaveTypeId"           INT,
    "LeaveTypeName"         VARCHAR,
    "FromDate"              TIMESTAMP,
    "ToDate"                TIMESTAMP,
    "TotalDays"             NUMERIC,
    "TotalHours"            NUMERIC,
    "Reason"                VARCHAR,
    "LeaveStatus"           INT,
    "LeaveStatusName"       VARCHAR,
    "ReportingManagerId"    INT,
    "ReportingManagerName"  VARCHAR,
    "HRApprovalRequired"    BOOLEAN,
    "ApprovedBy_L1"         INT,
    "ApprovedByL1Name"      VARCHAR,
    "ApprovedDate_L1"       TIMESTAMP,
    "ApprovedBy_HR"         INT,
    "ApprovedByHRName"      VARCHAR,
    "ApprovedDate_HR"       TIMESTAMP,
    "Attachment"            VARCHAR,
    "IsActive"              BOOLEAN,
    "CreatedDate"           TIMESTAMP
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        lr."RequestId",
        lr."StaffId",
        s."Name"          AS "StaffName",
        s."EmployeeCode",
        lr."RequestTypeId",
        rt."Name"         AS "RequestTypeName",
        lr."LeaveTypeId",
        lt."LeaveTypeName",
        lr."FromDate",
        lr."ToDate",
        lr."TotalDays",
        lr."TotalHours",
        lr."Reason",
        lr."LeaveStatus",
        ls."Name"         AS "LeaveStatusName",
        lr."ReportingManagerId",
        rm."Name"         AS "ReportingManagerName",
        lr."HRApprovalRequired",
        lr."ApprovedBy_L1",
        l1."Name"         AS "ApprovedByL1Name",
        lr."ApprovedDate_L1",
        lr."ApprovedBy_HR",
        hr."Name"         AS "ApprovedByHRName",
        lr."ApprovedDate_HR",
        lr."Attachment",
        lr."IsActive",
        lr."CreatedDate"
    FROM "Leave_OT_Request" lr
    INNER JOIN "Staff" s               ON lr."StaffId"            = s."StaffId"
    LEFT JOIN  "tbl_Master_Dropdown" rt ON lr."RequestTypeId"     = rt."Id"
    LEFT JOIN  "LeaveTypeMaster" lt     ON lr."LeaveTypeId"       = lt."LeaveTypeId"
    LEFT JOIN  "tbl_Master_Dropdown" ls ON lr."LeaveStatus"       = ls."Id"
    LEFT JOIN  "Staff" rm               ON lr."ReportingManagerId"= rm."StaffId"
    LEFT JOIN  "Staff" l1               ON lr."ApprovedBy_L1"     = l1."StaffId"
    LEFT JOIN  "Staff" hr               ON lr."ApprovedBy_HR"     = hr."StaffId"
    WHERE lr."TenantId" = p_tenant_id
      AND lr."StaffId"  = p_staff_id
      AND (p_is_active       IS NULL OR lr."IsActive"      = p_is_active)
      AND (p_request_type_id IS NULL OR lr."RequestTypeId" = p_request_type_id)
      AND (p_year            IS NULL OR EXTRACT(YEAR FROM lr."FromDate") = p_year)
    ORDER BY lr."CreatedDate" DESC;
END;
$$;
