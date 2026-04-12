using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class CourseAttendanceConfiguration : IEntityTypeConfiguration<CourseAttendance>
{
    public void Configure(EntityTypeBuilder<CourseAttendance> builder)
    {
        builder.ToTable("Course_Attendance");

        builder.HasKey(e => e.AttendanceId);

        builder.Property(e => e.AttendanceId)
            .HasColumnName("AttendanceId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("UserId");

        builder.Property(e => e.CoursePlanId)
            .IsRequired()
            .HasColumnName("CoursePlanId");

        builder.Property(e => e.StaffId)
            .IsRequired()
            .HasColumnName("StaffId");

        builder.Property(e => e.AttendanceDate)
            .IsRequired()
            .HasColumnName("AttendanceDate")
            .HasDefaultValueSql("NOW()")
            .HasColumnType("timestamp");

        builder.Property(e => e.CheckInTime)
            .HasColumnName("CheckInTime")
            .HasColumnType("time(7)");

        builder.Property(e => e.CheckOutTime)
            .HasColumnName("CheckOutTime")
            .HasColumnType("time(7)");

        builder.Property(e => e.Status)
            .HasMaxLength(50)
            .HasColumnName("Status")
            .HasDefaultValue("Present");

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

        // Indexes for better query performance
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("IX_CourseAttendance_TenantId");

        builder.HasIndex(e => e.CoursePlanId)
            .HasDatabaseName("IX_CourseAttendance_CoursePlanId");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_CourseAttendance_UserId");

        builder.HasIndex(e => e.StaffId)
            .HasDatabaseName("IX_CourseAttendance_StaffId");

        builder.HasIndex(e => e.AttendanceDate)
            .HasDatabaseName("IX_CourseAttendance_AttendanceDate");

        // Navigation property relationships
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

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
