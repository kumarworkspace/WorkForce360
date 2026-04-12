using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IPAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "HolidayMaster",
                columns: table => new
                {
                    HolidayId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HolidayDate = table.Column<DateTime>(type: "date", nullable: false),
                    HolidayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayMaster", x => x.HolidayId);
                });

            migrationBuilder.CreateTable(
                name: "LeaveTypeMaster",
                columns: table => new
                {
                    LeaveTypeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LeaveTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MaxDaysPerYear = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypeMaster", x => x.LeaveTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    StaffId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: true),
                    GenderId = table.Column<int>(type: "integer", nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IdentityCard = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Division = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Department = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Position = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EmploymentStatusId = table.Column<int>(type: "integer", nullable: true),
                    DateJoined = table.Column<DateTime>(type: "date", nullable: true),
                    RetirementDate = table.Column<DateTime>(type: "date", nullable: true),
                    Photo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReportingManager = table.Column<int>(type: "integer", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.StaffId);
                    table.ForeignKey(
                        name: "FK_Staff_Staff_ReportingManager",
                        column: x => x.ReportingManager,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Master_Dropdown",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Master_Dropdown", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SSO",
                columns: table => new
                {
                    SSOId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AccessToken = table.Column<string>(type: "text", nullable: true),
                    TokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SSO", x => x.SSOId);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Domain = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "EducationDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Institution = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Qualification = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    YearOfPassing = table.Column<int>(type: "integer", nullable: true),
                    GradeOrPercentage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationDetails_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExperienceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Company = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Position = table.Column<string>(type: "character varying(155)", maxLength: 155, nullable: true),
                    TotalExperience = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperienceDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExperienceDetails_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leave_OT_Request",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    RequestTypeId = table.Column<int>(type: "integer", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "integer", nullable: true),
                    FromDate = table.Column<DateTime>(type: "date", nullable: false),
                    ToDate = table.Column<DateTime>(type: "date", nullable: false),
                    TotalDays = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    TotalHours = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LeaveStatus = table.Column<int>(type: "integer", nullable: true),
                    ReportingManagerId = table.Column<int>(type: "integer", nullable: true),
                    HRApprovalRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApprovedBy_L1 = table.Column<int>(type: "integer", nullable: true),
                    ApprovedDate_L1 = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ApprovedBy_HR = table.Column<int>(type: "integer", nullable: true),
                    ApprovedDate_HR = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Attachment = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leave_OT_Request", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_Leave_OT_Request_LeaveTypeMaster_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypeMaster",
                        principalColumn: "LeaveTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leave_OT_Request_Staff_ReportingManagerId",
                        column: x => x.ReportingManagerId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leave_OT_Request_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveBalance",
                columns: table => new
                {
                    LeaveBalanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    LeaveTypeId = table.Column<int>(type: "integer", nullable: false),
                    TotalDays = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    UsedDays = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    RemainingDays = table.Column<decimal>(type: "numeric(5,2)", nullable: false, computedColumnSql: "\"TotalDays\" - \"UsedDays\"", stored: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveBalance", x => x.LeaveBalanceId);
                    table.ForeignKey(
                        name: "FK_LeaveBalance_LeaveTypeMaster_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypeMaster",
                        principalColumn: "LeaveTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveBalance_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LegalDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalDocuments_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseRegistration",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CourseCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TrainingModule = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CourseTypeId = table.Column<int>(type: "integer", nullable: false),
                    CourseCategoryId = table.Column<int>(type: "integer", nullable: false),
                    TrainerId = table.Column<int>(type: "integer", nullable: false),
                    Duration = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ValidityPeriod = table.Column<int>(type: "integer", nullable: false),
                    UploadFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRegistration", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_CourseRegistration_Staff_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseRegistration_tbl_Master_Dropdown_CourseCategoryId",
                        column: x => x.CourseCategoryId,
                        principalTable: "tbl_Master_Dropdown",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseRegistration_tbl_Master_Dropdown_CourseTypeId",
                        column: x => x.CourseTypeId,
                        principalTable: "tbl_Master_Dropdown",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseRegistration_tbl_Master_Dropdown_ValidityPeriod",
                        column: x => x.ValidityPeriod,
                        principalTable: "tbl_Master_Dropdown",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LMSSkills",
                columns: table => new
                {
                    SkillId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSSkills", x => x.SkillId);
                    table.ForeignKey(
                        name: "FK_LMSSkills_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModuleName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                    table.ForeignKey(
                        name: "FK_Permissions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_Roles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LoginProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    StaffId = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoursePlanning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time(7) without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time(7) without time zone", nullable: false),
                    Venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TrainerId = table.Column<int>(type: "integer", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadFilePaths = table.Column<string>(type: "text", nullable: true),
                    QRCodePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePlanning", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoursePlanning_CourseRegistration_CourseId",
                        column: x => x.CourseId,
                        principalTable: "CourseRegistration",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePlanning_Staff_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LMSCourses",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CourseCategoryId = table.Column<int>(type: "integer", nullable: true),
                    SkillId = table.Column<int>(type: "integer", nullable: true),
                    DurationHours = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    DifficultyLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSCourses", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_LMSCourses_LMSSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "LMSSkills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LMSCourses_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LMSCourses_tbl_Master_Dropdown_CourseCategoryId",
                        column: x => x.CourseCategoryId,
                        principalTable: "tbl_Master_Dropdown",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LMSEmployeeSkills",
                columns: table => new
                {
                    EmployeeSkillId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    SkillId = table.Column<int>(type: "integer", nullable: false),
                    ProficiencyLevel = table.Column<string>(type: "text", nullable: true),
                    LastAssessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextAssessmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSEmployeeSkills", x => x.EmployeeSkillId);
                    table.ForeignKey(
                        name: "FK_LMSEmployeeSkills_LMSSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "LMSSkills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSEmployeeSkills_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSEmployeeSkills_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false),
                    AccessLevel = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Course_Attendance",
                columns: table => new
                {
                    AttendanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CoursePlanId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CheckInTime = table.Column<TimeSpan>(type: "time(7) without time zone", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "time(7) without time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Present"),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course_Attendance", x => x.AttendanceId);
                    table.ForeignKey(
                        name: "FK_Course_Attendance_CoursePlanning_CoursePlanId",
                        column: x => x.CoursePlanId,
                        principalTable: "CoursePlanning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Course_Attendance_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Course_Attendance_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseAttendance_DateWise",
                columns: table => new
                {
                    AttendanceId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoursePlanId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsPresent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseAttendance_DateWise", x => x.AttendanceId);
                    table.ForeignKey(
                        name: "FK_CourseAttendance_DateWise_CoursePlanning_CoursePlanId",
                        column: x => x.CoursePlanId,
                        principalTable: "CoursePlanning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseAttendance_DateWise_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseParticipant",
                columns: table => new
                {
                    CourseParticipantId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoursePlanId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseParticipant", x => x.CourseParticipantId);
                    table.ForeignKey(
                        name: "FK_CourseParticipant_CoursePlanning_CoursePlanId",
                        column: x => x.CoursePlanId,
                        principalTable: "CoursePlanning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseParticipant_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseResult",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CoursePlanId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    TotalDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PresentDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AttendancePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    Marks = table.Column<decimal>(type: "numeric", nullable: true),
                    ResultStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CertificatePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CertificateSerialNumber = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseResult", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_CourseResult_CoursePlanning_CoursePlanId",
                        column: x => x.CoursePlanId,
                        principalTable: "CoursePlanning",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseResult_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LMSRecommendations",
                columns: table => new
                {
                    RecommendationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    RecommendationReason = table.Column<string>(type: "text", nullable: true),
                    RecommendationScore = table.Column<decimal>(type: "numeric", nullable: true),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSRecommendations", x => x.RecommendationId);
                    table.ForeignKey(
                        name: "FK_LMSRecommendations_LMSCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "LMSCourses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSRecommendations_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSRecommendations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LMSCertificates",
                columns: table => new
                {
                    CertificateId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificateUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QRCode = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSCertificates", x => x.CertificateId);
                    table.ForeignKey(
                        name: "FK_LMSCertificates_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LMSEnrollments",
                columns: table => new
                {
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    CompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Enrolled"),
                    ProgressPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    CertificateId = table.Column<int>(type: "integer", nullable: true),
                    IsRecommended = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSEnrollments", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_LMSEnrollments_LMSCertificates_CertificateId",
                        column: x => x.CertificateId,
                        principalTable: "LMSCertificates",
                        principalColumn: "CertificateId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LMSEnrollments_LMSCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "LMSCourses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LMSEnrollments_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LMSEnrollments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LMSModules",
                columns: table => new
                {
                    ModuleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    ContentUrl = table.Column<string>(type: "text", nullable: true),
                    VideoUrl = table.Column<string>(type: "text", nullable: true),
                    DocumentPath = table.Column<string>(type: "text", nullable: true),
                    QuizId = table.Column<int>(type: "integer", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSModules", x => x.ModuleId);
                    table.ForeignKey(
                        name: "FK_LMSModules_LMSCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "LMSCourses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSModules_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LMSProgress",
                columns: table => new
                {
                    ProgressId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeSpentMinutes = table.Column<int>(type: "integer", nullable: false),
                    LastAccessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSProgress", x => x.ProgressId);
                    table.ForeignKey(
                        name: "FK_LMSProgress_LMSEnrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "LMSEnrollments",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSProgress_LMSModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "LMSModules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSProgress_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LMSQuizzes",
                columns: table => new
                {
                    QuizId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CourseId = table.Column<int>(type: "integer", nullable: true),
                    ModuleId = table.Column<int>(type: "integer", nullable: true),
                    PassingScore = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSQuizzes", x => x.QuizId);
                    table.ForeignKey(
                        name: "FK_LMSQuizzes_LMSCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "LMSCourses",
                        principalColumn: "CourseId");
                    table.ForeignKey(
                        name: "FK_LMSQuizzes_LMSModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "LMSModules",
                        principalColumn: "ModuleId");
                    table.ForeignKey(
                        name: "FK_LMSQuizzes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LMSQuizAttempts",
                columns: table => new
                {
                    AttemptId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false),
                    QuizId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: true),
                    Answers = table.Column<string>(type: "text", nullable: true),
                    TimeSpentMinutes = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSQuizAttempts", x => x.AttemptId);
                    table.ForeignKey(
                        name: "FK_LMSQuizAttempts_LMSEnrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "LMSEnrollments",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSQuizAttempts_LMSQuizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "LMSQuizzes",
                        principalColumn: "QuizId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSQuizAttempts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LMSQuizQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuizId = table.Column<int>(type: "integer", nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    QuestionType = table.Column<string>(type: "text", nullable: false),
                    Options = table.Column<string>(type: "text", nullable: true),
                    CorrectAnswer = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LMSQuizQuestions", x => x.QuestionId);
                    table.ForeignKey(
                        name: "FK_LMSQuizQuestions_LMSQuizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "LMSQuizzes",
                        principalColumn: "QuizId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LMSQuizQuestions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendance_AttendanceDate",
                table: "Course_Attendance",
                column: "AttendanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendance_CoursePlanId",
                table: "Course_Attendance",
                column: "CoursePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendance_StaffId",
                table: "Course_Attendance",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendance_TenantId",
                table: "Course_Attendance",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendance_UserId",
                table: "Course_Attendance",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendanceDateWise_AttendanceDate",
                table: "CourseAttendance_DateWise",
                column: "AttendanceDate");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendanceDateWise_CoursePlanId",
                table: "CourseAttendance_DateWise",
                column: "CoursePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendanceDateWise_StaffId",
                table: "CourseAttendance_DateWise",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseAttendanceDateWise_TenantId",
                table: "CourseAttendance_DateWise",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UQ_CourseAttendanceDateWise_Course_Staff_Date",
                table: "CourseAttendance_DateWise",
                columns: new[] { "CoursePlanId", "StaffId", "AttendanceDate", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseParticipant_CoursePlanId",
                table: "CourseParticipant",
                column: "CoursePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseParticipant_StaffId",
                table: "CourseParticipant",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseParticipant_TenantId",
                table: "CourseParticipant",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UQ_CourseParticipant_Course_Staff",
                table: "CourseParticipant",
                columns: new[] { "CoursePlanId", "StaffId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlanning_CourseId",
                table: "CoursePlanning",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlanning_StartDate",
                table: "CoursePlanning",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlanning_TenantId",
                table: "CoursePlanning",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePlanning_TrainerId",
                table: "CoursePlanning",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistration_CourseCategoryId",
                table: "CourseRegistration",
                column: "CourseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistration_CourseTypeId",
                table: "CourseRegistration",
                column: "CourseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistration_TenantId_Code",
                table: "CourseRegistration",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistration_TrainerId",
                table: "CourseRegistration",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRegistration_ValidityPeriod",
                table: "CourseRegistration",
                column: "ValidityPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResult_CoursePlanId",
                table: "CourseResult",
                column: "CoursePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResult_ResultStatus",
                table: "CourseResult",
                column: "ResultStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResult_StaffId",
                table: "CourseResult",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseResult_TenantId",
                table: "CourseResult",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UQ_CourseResult_Course_Staff",
                table: "CourseResult",
                columns: new[] { "CoursePlanId", "StaffId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EducationDetails_StaffId",
                table: "EducationDetails",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperienceDetails_StaffId",
                table: "ExperienceDetails",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_HolidayMaster_TenantId_HolidayDate",
                table: "HolidayMaster",
                columns: new[] { "TenantId", "HolidayDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leave_OT_Request_LeaveTypeId",
                table: "Leave_OT_Request",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Leave_OT_Request_ReportingManagerId",
                table: "Leave_OT_Request",
                column: "ReportingManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Leave_OT_Request_StaffId",
                table: "Leave_OT_Request",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalance_LeaveTypeId",
                table: "LeaveBalance",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalance_StaffId",
                table: "LeaveBalance",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalance_TenantId_StaffId_LeaveTypeId_Year",
                table: "LeaveBalance",
                columns: new[] { "TenantId", "StaffId", "LeaveTypeId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypeMaster_TenantId_LeaveTypeName",
                table: "LeaveTypeMaster",
                columns: new[] { "TenantId", "LeaveTypeName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_StaffId",
                table: "LegalDocuments",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSCertificates_EnrollmentId",
                table: "LMSCertificates",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSCertificates_Number_Tenant",
                table: "LMSCertificates",
                columns: new[] { "CertificateNumber", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LMSCertificates_TenantId",
                table: "LMSCertificates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSCourses_CourseCategoryId",
                table: "LMSCourses",
                column: "CourseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSCourses_IsActive",
                table: "LMSCourses",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LMSCourses_SkillId",
                table: "LMSCourses",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSCourses_TenantId",
                table: "LMSCourses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSEmployeeSkills_SkillId",
                table: "LMSEmployeeSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSEmployeeSkills_StaffId",
                table: "LMSEmployeeSkills",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSEmployeeSkills_TenantId",
                table: "LMSEmployeeSkills",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSEnrollments_CertificateId",
                table: "LMSEnrollments",
                column: "CertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSEnrollments_CourseId",
                table: "LMSEnrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSEnrollments_StaffId",
                table: "LMSEnrollments",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSEnrollments_TenantId",
                table: "LMSEnrollments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSModules_CourseId",
                table: "LMSModules",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSModules_QuizId",
                table: "LMSModules",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSModules_TenantId",
                table: "LMSModules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSProgress_EnrollmentId",
                table: "LMSProgress",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSProgress_ModuleId",
                table: "LMSProgress",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSProgress_TenantId",
                table: "LMSProgress",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSQuizAttempts_EnrollmentId",
                table: "LMSQuizAttempts",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSQuizAttempts_QuizId",
                table: "LMSQuizAttempts",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSQuizAttempts_TenantId",
                table: "LMSQuizAttempts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSQuizQuestions_QuizId",
                table: "LMSQuizQuestions",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSQuizQuestions_TenantId",
                table: "LMSQuizQuestions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSQuizzes_CourseId",
                table: "LMSQuizzes",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSQuizzes_ModuleId",
                table: "LMSQuizzes",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSQuizzes_TenantId",
                table: "LMSQuizzes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSRecommendations_CourseId",
                table: "LMSRecommendations",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSRecommendations_StaffId",
                table: "LMSRecommendations",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSRecommendations_TenantId",
                table: "LMSRecommendations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LMSSkills_TenantId",
                table: "LMSSkills",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_TenantId_ModuleName",
                table: "Permissions",
                columns: new[] { "TenantId", "ModuleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionId_TenantId",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_TenantId",
                table: "RolePermissions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_RoleName",
                table: "Roles",
                columns: new[] { "TenantId", "RoleName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ReportingManager",
                table: "Staff",
                column: "ReportingManager");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_TenantId_Email",
                table: "Staff",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CompanyName",
                table: "Tenants",
                column: "CompanyName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_TenantId",
                table: "UserRoles",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId_TenantId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LMSCertificates_LMSEnrollments_EnrollmentId",
                table: "LMSCertificates",
                column: "EnrollmentId",
                principalTable: "LMSEnrollments",
                principalColumn: "EnrollmentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LMSModules_LMSQuizzes_QuizId",
                table: "LMSModules",
                column: "QuizId",
                principalTable: "LMSQuizzes",
                principalColumn: "QuizId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LMSEnrollments_Staff_StaffId",
                table: "LMSEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSCourses_tbl_Master_Dropdown_CourseCategoryId",
                table: "LMSCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSCertificates_LMSEnrollments_EnrollmentId",
                table: "LMSCertificates");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSCourses_Tenants_TenantId",
                table: "LMSCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSModules_Tenants_TenantId",
                table: "LMSModules");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSQuizzes_Tenants_TenantId",
                table: "LMSQuizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSSkills_Tenants_TenantId",
                table: "LMSSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSCourses_LMSSkills_SkillId",
                table: "LMSCourses");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSModules_LMSCourses_CourseId",
                table: "LMSModules");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSQuizzes_LMSCourses_CourseId",
                table: "LMSQuizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_LMSModules_LMSQuizzes_QuizId",
                table: "LMSModules");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Course_Attendance");

            migrationBuilder.DropTable(
                name: "CourseAttendance_DateWise");

            migrationBuilder.DropTable(
                name: "CourseParticipant");

            migrationBuilder.DropTable(
                name: "CourseResult");

            migrationBuilder.DropTable(
                name: "EducationDetails");

            migrationBuilder.DropTable(
                name: "ExperienceDetails");

            migrationBuilder.DropTable(
                name: "HolidayMaster");

            migrationBuilder.DropTable(
                name: "Leave_OT_Request");

            migrationBuilder.DropTable(
                name: "LeaveBalance");

            migrationBuilder.DropTable(
                name: "LegalDocuments");

            migrationBuilder.DropTable(
                name: "LMSEmployeeSkills");

            migrationBuilder.DropTable(
                name: "LMSProgress");

            migrationBuilder.DropTable(
                name: "LMSQuizAttempts");

            migrationBuilder.DropTable(
                name: "LMSQuizQuestions");

            migrationBuilder.DropTable(
                name: "LMSRecommendations");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "tbl_SSO");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "CoursePlanning");

            migrationBuilder.DropTable(
                name: "LeaveTypeMaster");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "CourseRegistration");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "tbl_Master_Dropdown");

            migrationBuilder.DropTable(
                name: "LMSEnrollments");

            migrationBuilder.DropTable(
                name: "LMSCertificates");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "LMSSkills");

            migrationBuilder.DropTable(
                name: "LMSCourses");

            migrationBuilder.DropTable(
                name: "LMSQuizzes");

            migrationBuilder.DropTable(
                name: "LMSModules");
        }
    }
}
