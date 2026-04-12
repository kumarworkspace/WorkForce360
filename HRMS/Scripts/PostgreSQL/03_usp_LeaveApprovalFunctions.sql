-- =============================================
-- PostgreSQL Functions: Leave Approval stored procedures
-- =============================================

-- =============================================
-- Function: usp_GetLeaveApprovalList
-- Returns single result set with TotalCount (COUNT(*) OVER())
-- =============================================
DROP FUNCTION IF EXISTS usp_GetLeaveApprovalList(INT, INT, BOOLEAN, INT, INT, INT, INT, DATE, DATE, VARCHAR, BOOLEAN, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetLeaveApprovalList(
    p_tenant_id            INT,
    p_reporting_manager_id INT     DEFAULT NULL,
    p_for_hr_approval      BOOLEAN DEFAULT FALSE,
    p_pending_status_id    INT     DEFAULT NULL,
    p_approved_status_id   INT     DEFAULT NULL,
    p_leave_status         INT     DEFAULT NULL,
    p_request_type_id      INT     DEFAULT NULL,
    p_from_date            DATE    DEFAULT NULL,
    p_to_date              DATE    DEFAULT NULL,
    p_search_term          VARCHAR(200) DEFAULT NULL,
    p_show_all_requests    BOOLEAN DEFAULT FALSE,
    p_page_number          INT     DEFAULT 1,
    p_page_size            INT     DEFAULT 10
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
    "StaffPhoto"            VARCHAR,
    "RequestTypeId"         INT,
    "RequestTypeName"       VARCHAR,
    "RequestTypeCode"       VARCHAR,
    "LeaveTypeId"           INT,
    "LeaveTypeName"         VARCHAR,
    "FromDate"              TIMESTAMP,
    "ToDate"                TIMESTAMP,
    "TotalDays"             NUMERIC,
    "TotalHours"            NUMERIC,
    "Reason"                VARCHAR,
    "LeaveStatus"           INT,
    "LeaveStatusName"       VARCHAR,
    "LeaveStatusCode"       VARCHAR,
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
    "CreatedDate"           TIMESTAMP,
    "CreatedBy"             INT,
    "ApprovalStage"         VARCHAR,
    "DaysUntilStart"        INT
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
        s."Photo"             AS "StaffPhoto",
        lr."RequestTypeId",
        rt."Name"             AS "RequestTypeName",
        rt."Code"             AS "RequestTypeCode",
        lr."LeaveTypeId",
        lt."LeaveTypeName",
        lr."FromDate",
        lr."ToDate",
        lr."TotalDays",
        lr."TotalHours",
        lr."Reason",
        lr."LeaveStatus",
        ls."Name"             AS "LeaveStatusName",
        ls."Code"             AS "LeaveStatusCode",
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
        lr."CreatedDate",
        lr."CreatedBy",
        CASE
            WHEN lr."ApprovedBy_L1" IS NULL
                 AND p_pending_status_id IS NOT NULL
                 AND lr."LeaveStatus" = p_pending_status_id                   THEN 'Pending L1'
            WHEN lr."ApprovedBy_L1" IS NOT NULL
                 AND lr."HRApprovalRequired" = TRUE
                 AND lr."ApprovedBy_HR" IS NULL                               THEN 'Pending HR'
            WHEN lr."ApprovedBy_HR" IS NOT NULL                               THEN 'Fully Approved'
            WHEN lr."ApprovedBy_L1" IS NOT NULL
                 AND lr."HRApprovalRequired" = FALSE                          THEN 'Approved'
            ELSE 'Unknown'
        END::VARCHAR                                                           AS "ApprovalStage",
        (lr."FromDate"::DATE - CURRENT_DATE)::INT                             AS "DaysUntilStart"
    FROM "Leave_OT_Request" lr
    INNER JOIN "Staff" s               ON lr."StaffId"             = s."StaffId"
    LEFT JOIN  "tbl_Master_Dropdown" rt ON lr."RequestTypeId"      = rt."Id"
    LEFT JOIN  "LeaveTypeMaster" lt     ON lr."LeaveTypeId"        = lt."LeaveTypeId"
    LEFT JOIN  "tbl_Master_Dropdown" ls ON lr."LeaveStatus"        = ls."Id"
    LEFT JOIN  "Staff" rm               ON lr."ReportingManagerId" = rm."StaffId"
    LEFT JOIN  "Staff" l1               ON lr."ApprovedBy_L1"      = l1."StaffId"
    LEFT JOIN  "Staff" hr               ON lr."ApprovedBy_HR"      = hr."StaffId"
    WHERE lr."TenantId" = p_tenant_id
      AND lr."IsActive" = TRUE
      AND (p_reporting_manager_id IS NULL OR lr."ReportingManagerId" = p_reporting_manager_id)
      AND (NOT p_for_hr_approval OR (
              lr."HRApprovalRequired" = TRUE
              AND lr."ApprovedBy_HR" IS NULL
              AND (lr."LeaveStatus" = p_pending_status_id OR lr."LeaveStatus" = p_approved_status_id)
          ))
      AND (p_leave_status    IS NULL OR lr."LeaveStatus"   = p_leave_status)
      AND (p_request_type_id IS NULL OR lr."RequestTypeId" = p_request_type_id)
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
-- Function: usp_GetPendingApprovalsByManager
-- =============================================
DROP FUNCTION IF EXISTS usp_GetPendingApprovalsByManager(INT, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetPendingApprovalsByManager(
    p_tenant_id            INT,
    p_reporting_manager_id INT,
    p_pending_status_id    INT
)
RETURNS TABLE (
    "RequestId"          INT,
    "StaffId"            INT,
    "StaffName"          VARCHAR,
    "EmployeeCode"       VARCHAR,
    "Department"         VARCHAR,
    "Division"           VARCHAR,
    "Position"           VARCHAR,
    "StaffPhoto"         VARCHAR,
    "RequestTypeId"      INT,
    "RequestTypeName"    VARCHAR,
    "RequestTypeCode"    VARCHAR,
    "LeaveTypeId"        INT,
    "LeaveTypeName"      VARCHAR,
    "FromDate"           TIMESTAMP,
    "ToDate"             TIMESTAMP,
    "TotalDays"          NUMERIC,
    "TotalHours"         NUMERIC,
    "Reason"             VARCHAR,
    "LeaveStatus"        INT,
    "LeaveStatusName"    VARCHAR,
    "HRApprovalRequired" BOOLEAN,
    "Attachment"         VARCHAR,
    "CreatedDate"        TIMESTAMP,
    "DaysUntilStart"     INT
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        lr."RequestId",
        lr."StaffId",
        s."Name"          AS "StaffName",
        s."EmployeeCode",
        s."Department",
        s."Division",
        s."Position",
        s."Photo"         AS "StaffPhoto",
        lr."RequestTypeId",
        rt."Name"         AS "RequestTypeName",
        rt."Code"         AS "RequestTypeCode",
        lr."LeaveTypeId",
        lt."LeaveTypeName",
        lr."FromDate",
        lr."ToDate",
        lr."TotalDays",
        lr."TotalHours",
        lr."Reason",
        lr."LeaveStatus",
        ls."Name"         AS "LeaveStatusName",
        lr."HRApprovalRequired",
        lr."Attachment",
        lr."CreatedDate",
        (lr."FromDate"::DATE - CURRENT_DATE)::INT AS "DaysUntilStart"
    FROM "Leave_OT_Request" lr
    INNER JOIN "Staff" s               ON lr."StaffId"            = s."StaffId"
    LEFT JOIN  "tbl_Master_Dropdown" rt ON lr."RequestTypeId"     = rt."Id"
    LEFT JOIN  "LeaveTypeMaster" lt     ON lr."LeaveTypeId"       = lt."LeaveTypeId"
    LEFT JOIN  "tbl_Master_Dropdown" ls ON lr."LeaveStatus"       = ls."Id"
    WHERE lr."TenantId"            = p_tenant_id
      AND lr."IsActive"            = TRUE
      AND lr."ReportingManagerId"  = p_reporting_manager_id
      AND lr."LeaveStatus"         = p_pending_status_id
    ORDER BY lr."CreatedDate" DESC;
END;
$$;

-- =============================================
-- Function: usp_GetPendingApprovalsForHR
-- =============================================
DROP FUNCTION IF EXISTS usp_GetPendingApprovalsForHR(INT, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetPendingApprovalsForHR(
    p_tenant_id          INT,
    p_pending_status_id  INT,
    p_approved_status_id INT
)
RETURNS TABLE (
    "RequestId"            INT,
    "StaffId"              INT,
    "StaffName"            VARCHAR,
    "EmployeeCode"         VARCHAR,
    "Department"           VARCHAR,
    "Division"             VARCHAR,
    "Position"             VARCHAR,
    "StaffPhoto"           VARCHAR,
    "RequestTypeId"        INT,
    "RequestTypeName"      VARCHAR,
    "RequestTypeCode"      VARCHAR,
    "LeaveTypeId"          INT,
    "LeaveTypeName"        VARCHAR,
    "FromDate"             TIMESTAMP,
    "ToDate"               TIMESTAMP,
    "TotalDays"            NUMERIC,
    "TotalHours"           NUMERIC,
    "Reason"               VARCHAR,
    "LeaveStatus"          INT,
    "LeaveStatusName"      VARCHAR,
    "ReportingManagerId"   INT,
    "ReportingManagerName" VARCHAR,
    "ApprovedBy_L1"        INT,
    "ApprovedByL1Name"     VARCHAR,
    "ApprovedDate_L1"      TIMESTAMP,
    "Attachment"           VARCHAR,
    "CreatedDate"          TIMESTAMP,
    "DaysUntilStart"       INT,
    "ApprovalStage"        VARCHAR
)
LANGUAGE plpgsql AS $$
BEGIN
    RETURN QUERY
    SELECT
        lr."RequestId",
        lr."StaffId",
        s."Name"          AS "StaffName",
        s."EmployeeCode",
        s."Department",
        s."Division",
        s."Position",
        s."Photo"         AS "StaffPhoto",
        lr."RequestTypeId",
        rt."Name"         AS "RequestTypeName",
        rt."Code"         AS "RequestTypeCode",
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
        lr."ApprovedBy_L1",
        l1."Name"         AS "ApprovedByL1Name",
        lr."ApprovedDate_L1",
        lr."Attachment",
        lr."CreatedDate",
        (lr."FromDate"::DATE - CURRENT_DATE)::INT AS "DaysUntilStart",
        CASE
            WHEN lr."ApprovedBy_L1" IS NULL THEN 'Pending L1'
            ELSE 'Pending HR'
        END::VARCHAR AS "ApprovalStage"
    FROM "Leave_OT_Request" lr
    INNER JOIN "Staff" s               ON lr."StaffId"             = s."StaffId"
    LEFT JOIN  "tbl_Master_Dropdown" rt ON lr."RequestTypeId"      = rt."Id"
    LEFT JOIN  "LeaveTypeMaster" lt     ON lr."LeaveTypeId"        = lt."LeaveTypeId"
    LEFT JOIN  "tbl_Master_Dropdown" ls ON lr."LeaveStatus"        = ls."Id"
    LEFT JOIN  "Staff" rm               ON lr."ReportingManagerId" = rm."StaffId"
    LEFT JOIN  "Staff" l1               ON lr."ApprovedBy_L1"      = l1."StaffId"
    WHERE lr."TenantId"            = p_tenant_id
      AND lr."IsActive"            = TRUE
      AND lr."HRApprovalRequired"  = TRUE
      AND lr."ApprovedBy_HR"       IS NULL
      AND (lr."LeaveStatus" = p_pending_status_id OR lr."LeaveStatus" = p_approved_status_id)
    ORDER BY lr."CreatedDate" DESC;
END;
$$;
