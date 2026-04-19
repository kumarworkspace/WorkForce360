using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HRMS.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_LmsCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningPaths",
                columns: table => new
                {
                    LearningPathId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    JobTitleId = table.Column<int>(type: "integer", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPaths", x => x.LearningPathId);
                    table.ForeignKey(
                        name: "FK_LearningPaths_MasterValues_JobTitleId",
                        column: x => x.JobTitleId,
                        principalTable: "MasterValues",
                        principalColumn: "MasterValueId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LearningPaths_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LmsCourses",
                columns: table => new
                {
                    LmsCourseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CourseTypeId = table.Column<int>(type: "integer", nullable: true),
                    DifficultyId = table.Column<int>(type: "integer", nullable: true),
                    ContentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ThumbnailPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Objectives = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LmsCourses", x => x.LmsCourseId);
                    table.ForeignKey(
                        name: "FK_LmsCourses_MasterValues_CourseTypeId",
                        column: x => x.CourseTypeId,
                        principalTable: "MasterValues",
                        principalColumn: "MasterValueId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LmsCourses_MasterValues_DifficultyId",
                        column: x => x.DifficultyId,
                        principalTable: "MasterValues",
                        principalColumn: "MasterValueId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LmsCourses_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffId = table.Column<int>(type: "integer", nullable: false),
                    LmsCourseId = table.Column<int>(type: "integer", nullable: false),
                    EnrolledDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Enrolled"),
                    CompletedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_Enrollments_LmsCourses_LmsCourseId",
                        column: x => x.LmsCourseId,
                        principalTable: "LmsCourses",
                        principalColumn: "LmsCourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LearningPathCourses",
                columns: table => new
                {
                    LearningPathCourseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LearningPathId = table.Column<int>(type: "integer", nullable: false),
                    LmsCourseId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPathCourses", x => x.LearningPathCourseId);
                    table.ForeignKey(
                        name: "FK_LearningPathCourses_LearningPaths_LearningPathId",
                        column: x => x.LearningPathId,
                        principalTable: "LearningPaths",
                        principalColumn: "LearningPathId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningPathCourses_LmsCourses_LmsCourseId",
                        column: x => x.LmsCourseId,
                        principalTable: "LmsCourses",
                        principalColumn: "LmsCourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LmsModules",
                columns: table => new
                {
                    LmsModuleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LmsCourseId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ContentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LmsModules", x => x.LmsModuleId);
                    table.ForeignKey(
                        name: "FK_LmsModules_LmsCourses_LmsCourseId",
                        column: x => x.LmsCourseId,
                        principalTable: "LmsCourses",
                        principalColumn: "LmsCourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressTracking",
                columns: table => new
                {
                    ProgressTrackingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnrollmentId = table.Column<int>(type: "integer", nullable: false),
                    LmsModuleId = table.Column<int>(type: "integer", nullable: false),
                    ProgressPct = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    LastAccessedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressTracking", x => x.ProgressTrackingId);
                    table.ForeignKey(
                        name: "FK_ProgressTracking_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "EnrollmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressTracking_LmsModules_LmsModuleId",
                        column: x => x.LmsModuleId,
                        principalTable: "LmsModules",
                        principalColumn: "LmsModuleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_LmsCourseId",
                table: "Enrollments",
                column: "LmsCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StaffId_LmsCourseId_TenantId",
                table: "Enrollments",
                columns: new[] { "StaffId", "LmsCourseId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StaffId_TenantId",
                table: "Enrollments",
                columns: new[] { "StaffId", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_TenantId",
                table: "Enrollments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathCourses_LearningPathId_LmsCourseId",
                table: "LearningPathCourses",
                columns: new[] { "LearningPathId", "LmsCourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningPathCourses_LmsCourseId",
                table: "LearningPathCourses",
                column: "LmsCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_JobTitleId",
                table: "LearningPaths",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_TenantId_IsActive",
                table: "LearningPaths",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LmsCourses_CourseTypeId",
                table: "LmsCourses",
                column: "CourseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LmsCourses_DifficultyId",
                table: "LmsCourses",
                column: "DifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_LmsCourses_TenantId_IsActive",
                table: "LmsCourses",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_LmsModules_LmsCourseId_SortOrder",
                table: "LmsModules",
                columns: new[] { "LmsCourseId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressTracking_EnrollmentId_LmsModuleId",
                table: "ProgressTracking",
                columns: new[] { "EnrollmentId", "LmsModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressTracking_LmsModuleId",
                table: "ProgressTracking",
                column: "LmsModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningPathCourses");

            migrationBuilder.DropTable(
                name: "ProgressTracking");

            migrationBuilder.DropTable(
                name: "LearningPaths");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "LmsModules");

            migrationBuilder.DropTable(
                name: "LmsCourses");
        }
    }
}
