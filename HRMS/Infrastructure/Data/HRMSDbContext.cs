using Microsoft.EntityFrameworkCore;
using HRMS.Core.Domain.Entities;
using HRMS.Core.Domain.Enums;
using HRMS.Core.Application.DTOs;
using HRMS.Infrastructure.Data.Configurations;

namespace HRMS.Infrastructure.Data;

public class HRMSDbContext : DbContext
{
    public HRMSDbContext(DbContextOptions<HRMSDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<SSO> SSOs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<EducationDetail> EducationDetails { get; set; }
    public DbSet<ExperienceDetail> ExperienceDetails { get; set; }
    public DbSet<LegalDocument> LegalDocuments { get; set; }
    public DbSet<MasterDropdown> MasterDropdowns { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<LeaveTypeMaster> LeaveTypeMasters { get; set; }
        public DbSet<HolidayMaster> HolidayMasters { get; set; }
        public DbSet<LeaveOtRequest> LeaveOtRequests { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
    public DbSet<CourseRegistration> CourseRegistrations { get; set; }
    public DbSet<CoursePlanning> CoursePlannings { get; set; }
    public DbSet<CourseAttendance> CourseAttendances { get; set; }
    public DbSet<CourseParticipant> CourseParticipants { get; set; }
    public DbSet<CourseAttendanceDateWise> CourseAttendancesDateWise { get; set; }
    public DbSet<CourseResult> CourseResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new SSOConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new StaffConfiguration());
        modelBuilder.ApplyConfiguration(new EducationDetailConfiguration());
        modelBuilder.ApplyConfiguration(new ExperienceDetailConfiguration());
        modelBuilder.ApplyConfiguration(new LegalDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new MasterDropdownConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveTypeMasterConfiguration());
        modelBuilder.ApplyConfiguration(new HolidayMasterConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveOtRequestConfiguration());
        modelBuilder.ApplyConfiguration(new LeaveBalanceConfiguration());
        modelBuilder.ApplyConfiguration(new CourseRegistrationConfiguration());
        modelBuilder.ApplyConfiguration(new CoursePlanningConfiguration());
        modelBuilder.ApplyConfiguration(new CourseAttendanceConfiguration());
        modelBuilder.ApplyConfiguration(new CourseParticipantConfiguration());
        modelBuilder.ApplyConfiguration(new CourseAttendanceDateWiseConfiguration());
        modelBuilder.ApplyConfiguration(new CourseResultConfiguration());

        // Configure keyless entities for stored procedure results
        modelBuilder.Entity<MarkAttendanceResponse>().HasNoKey().ToView(null);
        modelBuilder.Entity<CourseAttendanceDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<AttendanceSummaryDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<CourseParticipantDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<AttendanceDateWiseDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<CourseResultDto>().HasNoKey().ToView(null);

        // Staff List SP DTOs
        modelBuilder.Entity<StaffListSpDto>().HasNoKey().ToView(null);

        // Leave Request/Approval SP DTOs
        modelBuilder.Entity<LeaveRequestListSpDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<LeaveApprovalListSpDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<PendingApprovalByManagerDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<PendingApprovalForHRDto>().HasNoKey().ToView(null);

        // Attendance CRUD SP DTOs
        modelBuilder.Entity<AttendanceOperationResponse>().HasNoKey().ToView(null);
        modelBuilder.Entity<AttendanceSummaryByStaffDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<DailyAttendanceSummaryDto>().HasNoKey().ToView(null);

        // Staff Certificate DTOs
        modelBuilder.Entity<StaffCertificateDto>().HasNoKey().ToView(null);
    }
}
