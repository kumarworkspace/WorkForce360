-- =============================================
-- PostgreSQL Function: usp_GetStaffList
-- Description: Retrieves staff list with filters and pagination.
--   Returns a single result set with TotalCount as a window-function column.
-- =============================================

DROP FUNCTION IF EXISTS usp_GetStaffList(INT, VARCHAR, VARCHAR, VARCHAR, BOOLEAN, INT, INT);

CREATE OR REPLACE FUNCTION usp_GetStaffList(
    p_tenant_id     INT,
    p_search_term   VARCHAR(200) DEFAULT NULL,
    p_division      VARCHAR(100) DEFAULT NULL,
    p_department    VARCHAR(100) DEFAULT NULL,
    p_is_active     BOOLEAN      DEFAULT TRUE,
    p_page_number   INT          DEFAULT 1,
    p_page_size     INT          DEFAULT 10
)
RETURNS TABLE (
    "TotalCount"            INT,
    "StaffId"               INT,
    "EmployeeCode"          VARCHAR,
    "Name"                  VARCHAR,
    "Email"                 VARCHAR,
    "PhoneNumber"           VARCHAR,
    "Company"               VARCHAR,
    "Division"              VARCHAR,
    "Department"            VARCHAR,
    "Position"              VARCHAR,
    "DateOfBirth"           TIMESTAMP,
    "DateJoined"            TIMESTAMP,
    "RetirementDate"        TIMESTAMP,
    "Photo"                 VARCHAR,
    "Address"               VARCHAR,
    "IdentityCard"          VARCHAR,
    "GenderId"              INT,
    "GenderName"            VARCHAR,
    "EmploymentStatusId"    INT,
    "EmploymentStatusName"  VARCHAR,
    "ReportingManagerId"    INT,
    "ReportingManagerName"  VARCHAR,
    "ReportingManagerCode"  VARCHAR,
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
        COUNT(*) OVER()::INT              AS "TotalCount",
        s."StaffId",
        s."EmployeeCode",
        s."Name",
        s."Email",
        s."PhoneNumber",
        s."Company",
        s."Division",
        s."Department",
        s."Position",
        s."DateOfBirth",
        s."DateJoined",
        s."RetirementDate",
        s."Photo",
        s."Address",
        s."IdentityCard",
        s."GenderId",
        g."Name"                          AS "GenderName",
        s."EmploymentStatusId",
        es."Name"                         AS "EmploymentStatusName",
        s."ReportingManagerId",
        rm."Name"                         AS "ReportingManagerName",
        rm."EmployeeCode"                 AS "ReportingManagerCode",
        s."IsActive",
        s."TenantId",
        s."CreatedDate",
        s."CreatedBy",
        s."UpdatedDate",
        s."UpdatedBy"
    FROM "Staff" s
    LEFT JOIN "tbl_Master_Dropdown" g  ON s."GenderId"           = g."Id" AND g."Category" = 'Gender'
    LEFT JOIN "tbl_Master_Dropdown" es ON s."EmploymentStatusId" = es."Id" AND es."Category" = 'EmploymentStatus'
    LEFT JOIN "Staff" rm               ON s."ReportingManagerId" = rm."StaffId"
    WHERE s."TenantId" = p_tenant_id
      AND (p_is_active IS NULL OR s."IsActive" = p_is_active)
      AND (p_division   IS NULL OR p_division   = '' OR s."Division"   = p_division)
      AND (p_department IS NULL OR p_department = '' OR s."Department" = p_department)
      AND (p_search_term IS NULL OR p_search_term = '' OR
           s."Name"         ILIKE '%' || p_search_term || '%' OR
           s."EmployeeCode" ILIKE '%' || p_search_term || '%' OR
           s."Email"        ILIKE '%' || p_search_term || '%' OR
           s."Division"     ILIKE '%' || p_search_term || '%' OR
           s."Department"   ILIKE '%' || p_search_term || '%')
    ORDER BY s."Name" ASC
    OFFSET v_offset
    LIMIT  p_page_size;
END;
$$;
