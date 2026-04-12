-- =============================================
-- Master DDL Script: HRMS/TMS/LMS PostgreSQL Schema
-- Description: Creates all tables migrated from SQL Server.
--   Run this script once on a fresh PostgreSQL database,
--   then run scripts 01-06 to create the functions.
-- =============================================

-- =============================================
-- Core / Identity Tables
-- =============================================

CREATE TABLE IF NOT EXISTS "Tenant" (
    "TenantId"    SERIAL PRIMARY KEY,
    "Name"        VARCHAR(200) NOT NULL,
    "Domain"      VARCHAR(200),
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"   INT,
    "UpdatedDate" TIMESTAMP,
    "UpdatedBy"   INT
);

CREATE TABLE IF NOT EXISTS "Users" (
    "UserId"      SERIAL PRIMARY KEY,
    "TenantId"    INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "Email"       VARCHAR(200) NOT NULL,
    "PasswordHash" VARCHAR(500),
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"   INT,
    "UpdatedDate" TIMESTAMP,
    "UpdatedBy"   INT
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_TenantId_Email" ON "Users"("TenantId", "Email");

CREATE TABLE IF NOT EXISTS "AuditLog" (
    "AuditLogId"  SERIAL PRIMARY KEY,
    "TenantId"    INT NOT NULL,
    "UserId"      INT,
    "Action"      VARCHAR(100),
    "TableName"   VARCHAR(100),
    "RecordId"    INT,
    "OldValues"   TEXT,
    "NewValues"   TEXT,
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "SSO" (
    "SSOId"       SERIAL PRIMARY KEY,
    "TenantId"    INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "Provider"    VARCHAR(100),
    "ClientId"    VARCHAR(200),
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =============================================
-- RBAC Tables
-- =============================================

CREATE TABLE IF NOT EXISTS "Role" (
    "RoleId"      SERIAL PRIMARY KEY,
    "TenantId"    INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "Name"        VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"   INT,
    "UpdatedDate" TIMESTAMP,
    "UpdatedBy"   INT
);

CREATE TABLE IF NOT EXISTS "Permission" (
    "PermissionId" SERIAL PRIMARY KEY,
    "Name"         VARCHAR(100) NOT NULL,
    "Description"  VARCHAR(500),
    "Module"       VARCHAR(100),
    "IsActive"     BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS "RolePermission" (
    "RolePermissionId" SERIAL PRIMARY KEY,
    "RoleId"           INT NOT NULL REFERENCES "Role"("RoleId"),
    "PermissionId"     INT NOT NULL REFERENCES "Permission"("PermissionId")
);

CREATE TABLE IF NOT EXISTS "UserRole" (
    "UserRoleId" SERIAL PRIMARY KEY,
    "UserId"     INT NOT NULL REFERENCES "Users"("UserId"),
    "RoleId"     INT NOT NULL REFERENCES "Role"("RoleId"),
    "TenantId"   INT NOT NULL REFERENCES "Tenant"("TenantId")
);

-- =============================================
-- Master / Lookup Tables
-- =============================================

CREATE TABLE IF NOT EXISTS "MasterDropdown" (
    "Id"            SERIAL PRIMARY KEY,
    "TenantId"      INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "Category"      VARCHAR(100) NOT NULL,
    "Name"          VARCHAR(200) NOT NULL,
    "Code"          VARCHAR(50),
    "DropdownValue" VARCHAR(200),
    "SortOrder"     INT NOT NULL DEFAULT 0,
    "IsActive"      BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"   TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"     INT,
    "UpdatedDate"   TIMESTAMP,
    "UpdatedBy"     INT
);

-- Legacy alias used in SQL scripts
CREATE TABLE IF NOT EXISTS "tbl_Master_Dropdown" (
    "Id"            SERIAL PRIMARY KEY,
    "TenantId"      INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "Category"      VARCHAR(100) NOT NULL,
    "Name"          VARCHAR(200) NOT NULL,
    "Code"          VARCHAR(50),
    "DropdownValue" VARCHAR(200),
    "SortOrder"     INT NOT NULL DEFAULT 0,
    "IsActive"      BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"   TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"     INT,
    "UpdatedDate"   TIMESTAMP,
    "UpdatedBy"     INT
);

-- =============================================
-- Staff Tables
-- =============================================

CREATE TABLE IF NOT EXISTS "Staff" (
    "StaffId"            SERIAL PRIMARY KEY,
    "TenantId"           INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "EmployeeCode"       VARCHAR(50),
    "Name"               VARCHAR(200),
    "Email"              VARCHAR(200),
    "PhoneNumber"        VARCHAR(50),
    "Company"            VARCHAR(200),
    "Division"           VARCHAR(100),
    "Department"         VARCHAR(100),
    "Position"           VARCHAR(100),
    "DateOfBirth"        TIMESTAMP,
    "DateJoined"         TIMESTAMP,
    "RetirementDate"     TIMESTAMP,
    "Photo"              VARCHAR(500),
    "Address"            VARCHAR(500),
    "IdentityCard"       VARCHAR(100),
    "GenderId"           INT REFERENCES "tbl_Master_Dropdown"("Id"),
    "EmploymentStatusId" INT REFERENCES "tbl_Master_Dropdown"("Id"),
    "ReportingManagerId" INT REFERENCES "Staff"("StaffId"),
    "IsActive"           BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"        TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"          INT,
    "UpdatedDate"        TIMESTAMP,
    "UpdatedBy"          INT
);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Staff_TenantId_Email" ON "Staff"("TenantId", "Email") WHERE "Email" IS NOT NULL;

CREATE TABLE IF NOT EXISTS "EducationDetail" (
    "EducationDetailId" SERIAL PRIMARY KEY,
    "StaffId"           INT NOT NULL REFERENCES "Staff"("StaffId"),
    "TenantId"          INT NOT NULL,
    "Institution"       VARCHAR(200),
    "Degree"            VARCHAR(200),
    "FieldOfStudy"      VARCHAR(200),
    "StartYear"         INT,
    "EndYear"           INT,
    "IsActive"          BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"       TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"         INT,
    "UpdatedDate"       TIMESTAMP,
    "UpdatedBy"         INT
);

CREATE TABLE IF NOT EXISTS "ExperienceDetail" (
    "ExperienceDetailId" SERIAL PRIMARY KEY,
    "StaffId"            INT NOT NULL REFERENCES "Staff"("StaffId"),
    "TenantId"           INT NOT NULL,
    "Company"            VARCHAR(200),
    "Position"           VARCHAR(200),
    "StartDate"          TIMESTAMP,
    "EndDate"            TIMESTAMP,
    "IsActive"           BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"        TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"          INT,
    "UpdatedDate"        TIMESTAMP,
    "UpdatedBy"          INT
);

CREATE TABLE IF NOT EXISTS "LegalDocument" (
    "LegalDocumentId" SERIAL PRIMARY KEY,
    "StaffId"         INT NOT NULL REFERENCES "Staff"("StaffId"),
    "TenantId"        INT NOT NULL,
    "DocumentType"    VARCHAR(100),
    "DocumentNumber"  VARCHAR(100),
    "FilePath"        VARCHAR(500),
    "ExpiryDate"      TIMESTAMP,
    "IsActive"        BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"     TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"       INT,
    "UpdatedDate"     TIMESTAMP,
    "UpdatedBy"       INT
);

-- =============================================
-- Leave / OT Tables
-- =============================================

CREATE TABLE IF NOT EXISTS "LeaveTypeMaster" (
    "LeaveTypeId"  SERIAL PRIMARY KEY,
    "TenantId"     INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "LeaveTypeName" VARCHAR(200) NOT NULL,
    "MaxDaysPerYear" NUMERIC(5,2),
    "IsActive"     BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"  TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"    INT,
    "UpdatedDate"  TIMESTAMP,
    "UpdatedBy"    INT
);

CREATE TABLE IF NOT EXISTS "HolidayMaster" (
    "HolidayId"   SERIAL PRIMARY KEY,
    "TenantId"    INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "HolidayName" VARCHAR(200) NOT NULL,
    "HolidayDate" TIMESTAMP NOT NULL,
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"   INT,
    "UpdatedDate" TIMESTAMP,
    "UpdatedBy"   INT
);

CREATE TABLE IF NOT EXISTS "Leave_OT_Request" (
    "RequestId"           SERIAL PRIMARY KEY,
    "TenantId"            INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "StaffId"             INT NOT NULL REFERENCES "Staff"("StaffId"),
    "RequestTypeId"       INT REFERENCES "tbl_Master_Dropdown"("Id"),
    "LeaveTypeId"         INT REFERENCES "LeaveTypeMaster"("LeaveTypeId"),
    "FromDate"            TIMESTAMP NOT NULL,
    "ToDate"              TIMESTAMP NOT NULL,
    "TotalDays"           NUMERIC(5,2),
    "TotalHours"          NUMERIC(5,2),
    "Reason"              VARCHAR(1000),
    "LeaveStatus"         INT REFERENCES "tbl_Master_Dropdown"("Id"),
    "ReportingManagerId"  INT REFERENCES "Staff"("StaffId"),
    "HRApprovalRequired"  BOOLEAN NOT NULL DEFAULT FALSE,
    "ApprovedBy_L1"       INT REFERENCES "Staff"("StaffId"),
    "ApprovedDate_L1"     TIMESTAMP,
    "ApprovedBy_HR"       INT REFERENCES "Staff"("StaffId"),
    "ApprovedDate_HR"     TIMESTAMP,
    "Attachment"          VARCHAR(500),
    "IsActive"            BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"         TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"           INT,
    "UpdatedDate"         TIMESTAMP,
    "UpdatedBy"           INT
);

CREATE TABLE IF NOT EXISTS "LeaveBalance" (
    "LeaveBalanceId" SERIAL PRIMARY KEY,
    "TenantId"       INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "StaffId"        INT NOT NULL REFERENCES "Staff"("StaffId"),
    "LeaveTypeId"    INT NOT NULL REFERENCES "LeaveTypeMaster"("LeaveTypeId"),
    "Year"           INT NOT NULL,
    "Balance"        NUMERIC(5,2) NOT NULL DEFAULT 0,
    "Used"           NUMERIC(5,2) NOT NULL DEFAULT 0,
    "IsActive"       BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"    TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"      INT,
    "UpdatedDate"    TIMESTAMP,
    "UpdatedBy"      INT
);

-- =============================================
-- TMS / Course Tables
-- =============================================

CREATE TABLE IF NOT EXISTS "CourseRegistration" (
    "CourseId"         SERIAL PRIMARY KEY,
    "TenantId"         INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "Title"            VARCHAR(200) NOT NULL,
    "Code"             VARCHAR(50),
    "CourseCode"       VARCHAR(50),
    "Description"      TEXT,
    "Duration"         INT,
    "TrainingModule"   VARCHAR(200),
    "CourseTypeId"     INT REFERENCES "MasterDropdown"("Id"),
    "CourseCategoryId" INT REFERENCES "MasterDropdown"("Id"),
    "IsActive"         BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"      TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"        INT,
    "UpdatedDate"      TIMESTAMP,
    "UpdatedBy"        INT
);

CREATE TABLE IF NOT EXISTS "CoursePlanning" (
    "Id"              SERIAL PRIMARY KEY,
    "TenantId"        INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "CourseId"        INT NOT NULL REFERENCES "CourseRegistration"("CourseId"),
    "StartDate"       TIMESTAMP NOT NULL,
    "StartTime"       TIME NOT NULL,
    "EndDate"         TIMESTAMP NOT NULL,
    "EndTime"         TIME NOT NULL,
    "Venue"           VARCHAR(200) NOT NULL,
    "TrainerId"       INT NOT NULL REFERENCES "Staff"("StaffId"),
    "Remarks"         VARCHAR(500),
    "UploadFilePaths" TEXT,
    "QRCodePath"      VARCHAR(500),
    "IsActive"        BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"     TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"       INT,
    "UpdatedDate"     TIMESTAMP,
    "UpdatedBy"       INT
);
CREATE INDEX IF NOT EXISTS "IX_CoursePlanning_TenantId"  ON "CoursePlanning"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_CoursePlanning_CourseId"  ON "CoursePlanning"("CourseId");
CREATE INDEX IF NOT EXISTS "IX_CoursePlanning_TrainerId" ON "CoursePlanning"("TrainerId");
CREATE INDEX IF NOT EXISTS "IX_CoursePlanning_StartDate" ON "CoursePlanning"("StartDate");

CREATE TABLE IF NOT EXISTS "CourseAttendance" (
    "AttendanceId"  SERIAL PRIMARY KEY,
    "TenantId"      INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "CoursePlanId"  INT NOT NULL REFERENCES "CoursePlanning"("Id"),
    "UserId"        INT REFERENCES "Users"("UserId"),
    "StaffId"       INT REFERENCES "Staff"("StaffId"),
    "CheckInTime"   TIME,
    "IsActive"      BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"   TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"     INT,
    "UpdatedDate"   TIMESTAMP,
    "UpdatedBy"     INT
);

CREATE TABLE IF NOT EXISTS "CourseParticipant" (
    "CourseParticipantId" SERIAL PRIMARY KEY,
    "TenantId"            INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "CoursePlanId"        INT NOT NULL REFERENCES "CoursePlanning"("Id"),
    "StaffId"             INT NOT NULL REFERENCES "Staff"("StaffId"),
    "IsActive"            BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"         TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"           INT,
    "UpdatedDate"         TIMESTAMP,
    "UpdatedBy"           INT
);
CREATE INDEX IF NOT EXISTS "IX_CourseParticipant_TenantId"   ON "CourseParticipant"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_CourseParticipant_CoursePlanId" ON "CourseParticipant"("CoursePlanId");
CREATE INDEX IF NOT EXISTS "IX_CourseParticipant_StaffId"    ON "CourseParticipant"("StaffId");

CREATE TABLE IF NOT EXISTS "CourseAttendance_DateWise" (
    "AttendanceId"  SERIAL PRIMARY KEY,
    "TenantId"      INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "CoursePlanId"  INT NOT NULL REFERENCES "CoursePlanning"("Id"),
    "StaffId"       INT NOT NULL REFERENCES "Staff"("StaffId"),
    "AttendanceDate" TIMESTAMP NOT NULL,
    "IsPresent"     BOOLEAN NOT NULL DEFAULT TRUE,
    "Remarks"       VARCHAR(500),
    "IsActive"      BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"   TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"     INT,
    "UpdatedDate"   TIMESTAMP,
    "UpdatedBy"     INT
);
CREATE INDEX IF NOT EXISTS "IX_CourseAttendanceDateWise_TenantId"      ON "CourseAttendance_DateWise"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_CourseAttendanceDateWise_CoursePlanId"  ON "CourseAttendance_DateWise"("CoursePlanId");
CREATE INDEX IF NOT EXISTS "IX_CourseAttendanceDateWise_StaffId"       ON "CourseAttendance_DateWise"("StaffId");
CREATE INDEX IF NOT EXISTS "IX_CourseAttendanceDateWise_AttendanceDate" ON "CourseAttendance_DateWise"("AttendanceDate");

CREATE TABLE IF NOT EXISTS "CourseResult" (
    "ResultId"              SERIAL PRIMARY KEY,
    "TenantId"              INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "CoursePlanId"          INT NOT NULL REFERENCES "CoursePlanning"("Id"),
    "StaffId"               INT NOT NULL REFERENCES "Staff"("StaffId"),
    "TotalDays"             INT NOT NULL DEFAULT 0,
    "PresentDays"           INT NOT NULL DEFAULT 0,
    "AttendancePercentage"  NUMERIC(5,2) NOT NULL DEFAULT 0,
    "Marks"                 NUMERIC(5,2),
    "ResultStatus"          VARCHAR(10),
    "CertificatePath"       VARCHAR(500),
    "CertificateSerialNumber" VARCHAR(50),
    "IsActive"              BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"           TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"             INT,
    "UpdatedDate"           TIMESTAMP,
    "UpdatedBy"             INT
);
CREATE INDEX IF NOT EXISTS "IX_CourseResult_TenantId"    ON "CourseResult"("TenantId");
CREATE INDEX IF NOT EXISTS "IX_CourseResult_CoursePlanId" ON "CourseResult"("CoursePlanId");
CREATE INDEX IF NOT EXISTS "IX_CourseResult_StaffId"     ON "CourseResult"("StaffId");

-- =============================================
-- LMS Tables
-- =============================================

CREATE TABLE IF NOT EXISTS "LMSSkills" (
    "SkillId"     SERIAL PRIMARY KEY,
    "TenantId"    INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "Name"        VARCHAR(200) NOT NULL,
    "Description" TEXT,
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "LMSCourses" (
    "CourseId"         SERIAL PRIMARY KEY,
    "TenantId"         INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "Code"             VARCHAR(50) NOT NULL,
    "Title"            VARCHAR(200) NOT NULL,
    "Description"      TEXT,
    "CourseCategoryId" INT REFERENCES "MasterDropdown"("Id"),
    "SkillId"          INT REFERENCES "LMSSkills"("SkillId"),
    "DurationHours"    INT,
    "DifficultyLevel"  VARCHAR(20),
    "IsActive"         BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"      TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"        INT,
    "UpdatedDate"      TIMESTAMP,
    "UpdatedBy"        INT
);

CREATE TABLE IF NOT EXISTS "LMSQuizzes" (
    "QuizId"      SERIAL PRIMARY KEY,
    "TenantId"    INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "CourseId"    INT REFERENCES "LMSCourses"("CourseId"),
    "Title"       VARCHAR(200) NOT NULL,
    "PassScore"   INT NOT NULL DEFAULT 70,
    "IsActive"    BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate" TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "LMSModules" (
    "ModuleId"        SERIAL PRIMARY KEY,
    "TenantId"        INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "CourseId"        INT NOT NULL REFERENCES "LMSCourses"("CourseId"),
    "Title"           VARCHAR(200) NOT NULL,
    "Description"     TEXT,
    "ContentType"     VARCHAR(20) NOT NULL,
    "ContentUrl"      VARCHAR(500),
    "VideoUrl"        VARCHAR(500),
    "DocumentPath"    VARCHAR(500),
    "QuizId"          INT REFERENCES "LMSQuizzes"("QuizId"),
    "OrderIndex"      INT NOT NULL DEFAULT 0,
    "DurationMinutes" INT,
    "IsActive"        BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"     TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"       INT,
    "UpdatedDate"     TIMESTAMP,
    "UpdatedBy"       INT
);

CREATE TABLE IF NOT EXISTS "LMSCertificates" (
    "CertificateId"     SERIAL PRIMARY KEY,
    "TenantId"          INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "StaffId"           INT NOT NULL REFERENCES "Staff"("StaffId"),
    "CourseId"          INT NOT NULL REFERENCES "LMSCourses"("CourseId"),
    "CertificatePath"   VARCHAR(500),
    "IssuedDate"        TIMESTAMP NOT NULL DEFAULT NOW(),
    "ExpiryDate"        TIMESTAMP,
    "IsActive"          BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"       TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "LMSEnrollments" (
    "EnrollmentId"       SERIAL PRIMARY KEY,
    "TenantId"           INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "CourseId"           INT NOT NULL REFERENCES "LMSCourses"("CourseId"),
    "StaffId"            INT NOT NULL REFERENCES "Staff"("StaffId"),
    "EnrollmentDate"     TIMESTAMP NOT NULL DEFAULT NOW(),
    "CompletionDate"     TIMESTAMP,
    "Status"             VARCHAR(20) NOT NULL DEFAULT 'Enrolled',
    "ProgressPercentage" NUMERIC(5,2) DEFAULT 0,
    "CertificateId"      INT REFERENCES "LMSCertificates"("CertificateId"),
    "IsRecommended"      BOOLEAN NOT NULL DEFAULT FALSE,
    "IsActive"           BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"        TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy"          INT,
    "UpdatedDate"        TIMESTAMP,
    "UpdatedBy"          INT
);

CREATE TABLE IF NOT EXISTS "LMSProgress" (
    "ProgressId"   SERIAL PRIMARY KEY,
    "TenantId"     INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "EnrollmentId" INT NOT NULL REFERENCES "LMSEnrollments"("EnrollmentId"),
    "ModuleId"     INT NOT NULL REFERENCES "LMSModules"("ModuleId"),
    "IsCompleted"  BOOLEAN NOT NULL DEFAULT FALSE,
    "CompletedDate" TIMESTAMP,
    "IsActive"     BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"  TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "LMSQuizQuestions" (
    "QuestionId"    SERIAL PRIMARY KEY,
    "TenantId"      INT NOT NULL,
    "QuizId"        INT NOT NULL REFERENCES "LMSQuizzes"("QuizId"),
    "QuestionText"  TEXT NOT NULL,
    "OptionA"       VARCHAR(500),
    "OptionB"       VARCHAR(500),
    "OptionC"       VARCHAR(500),
    "OptionD"       VARCHAR(500),
    "CorrectAnswer" VARCHAR(1),
    "IsActive"      BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"   TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "LMSQuizAttempts" (
    "AttemptId"    SERIAL PRIMARY KEY,
    "TenantId"     INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "EnrollmentId" INT NOT NULL REFERENCES "LMSEnrollments"("EnrollmentId"),
    "QuizId"       INT NOT NULL REFERENCES "LMSQuizzes"("QuizId"),
    "Score"        INT,
    "IsPassed"     BOOLEAN,
    "AttemptDate"  TIMESTAMP NOT NULL DEFAULT NOW(),
    "IsActive"     BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS "LMSEmployeeSkills" (
    "EmployeeSkillId"  SERIAL PRIMARY KEY,
    "TenantId"         INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "StaffId"          INT NOT NULL REFERENCES "Staff"("StaffId"),
    "SkillId"          INT NOT NULL REFERENCES "LMSSkills"("SkillId"),
    "ProficiencyLevel" VARCHAR(20),
    "LastAssessedDate" TIMESTAMP,
    "IsActive"         BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"      TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS "LMSRecommendations" (
    "RecommendationId"     SERIAL PRIMARY KEY,
    "TenantId"             INT NOT NULL REFERENCES "Tenant"("TenantId"),
    "StaffId"              INT NOT NULL REFERENCES "Staff"("StaffId"),
    "CourseId"             INT NOT NULL REFERENCES "LMSCourses"("CourseId"),
    "RecommendationReason" VARCHAR(200),
    "RecommendationScore"  INT,
    "IsAccepted"           BOOLEAN,
    "IsActive"             BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedDate"          TIMESTAMP NOT NULL DEFAULT NOW()
);
