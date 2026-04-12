using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class CourseAttendanceDateWiseConfiguration : IEntityTypeConfiguration<CourseAttendanceDateWise>
{
    public void Configure(EntityTypeBuilder<CourseAttendanceDateWise> builder)
    {
        builder.ToTable("CourseAttendance_DateWise");

        builder.HasKey(e => e.AttendanceId);

        builder.Property(e => e.AttendanceId)
            .HasColumnName("AttendanceId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CoursePlanId)
            .IsRequired()
            .HasColumnName("CoursePlanId");

        builder.Property(e => e.StaffId)
            .IsRequired()
            .HasColumnName("StaffId");

        builder.Property(e => e.AttendanceDate)
            .IsRequired()
            .HasColumnName("AttendanceDate")
            .HasColumnType("date");

        builder.Property(e => e.IsPresent)
            .IsRequired()
            .HasColumnName("IsPresent")
            .HasDefaultValue(true);

        builder.Property(e => e.Remarks)
            .HasMaxLength(500)
            .HasColumnName("Remarks");

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasColumnName("CreatedDate")
            .HasDefaultValueSql("NOW()")
            .HasColumnType("timestamp");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("CreatedBy");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("UpdatedDate")
            .HasColumnType("timestamp");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UpdatedBy");

        // Indexes
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("IX_CourseAttendanceDateWise_TenantId");

        builder.HasIndex(e => e.CoursePlanId)
            .HasDatabaseName("IX_CourseAttendanceDateWise_CoursePlanId");

        builder.HasIndex(e => e.StaffId)
            .HasDatabaseName("IX_CourseAttendanceDateWise_StaffId");

        builder.HasIndex(e => e.AttendanceDate)
            .HasDatabaseName("IX_CourseAttendanceDateWise_AttendanceDate");

        // Unique constraint
        builder.HasIndex(e => new { e.CoursePlanId, e.StaffId, e.AttendanceDate, e.TenantId })
            .IsUnique()
            .HasDatabaseName("UQ_CourseAttendanceDateWise_Course_Staff_Date");

        // Navigation properties
        builder.HasOne(e => e.CoursePlan)
            .WithMany()
            .HasForeignKey(e => e.CoursePlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Staff)
            .WithMany()
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
