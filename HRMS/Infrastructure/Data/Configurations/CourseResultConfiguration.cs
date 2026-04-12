using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class CourseResultConfiguration : IEntityTypeConfiguration<CourseResult>
{
    public void Configure(EntityTypeBuilder<CourseResult> builder)
    {
        builder.ToTable("CourseResult");

        builder.HasKey(e => e.ResultId);

        builder.Property(e => e.ResultId)
            .HasColumnName("ResultId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CoursePlanId)
            .IsRequired()
            .HasColumnName("CoursePlanId");

        builder.Property(e => e.StaffId)
            .IsRequired()
            .HasColumnName("StaffId");

        builder.Property(e => e.TotalDays)
            .IsRequired()
            .HasColumnName("TotalDays")
            .HasDefaultValue(0);

        builder.Property(e => e.PresentDays)
            .IsRequired()
            .HasColumnName("PresentDays")
            .HasDefaultValue(0);

        builder.Property(e => e.AttendancePercentage)
            .IsRequired()
            .HasColumnName("AttendancePercentage")
            .HasColumnType("decimal(5,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.ResultStatus)
            .HasMaxLength(10)
            .HasColumnName("ResultStatus");

        builder.Property(e => e.CertificatePath)
            .HasMaxLength(500)
            .HasColumnName("CertificatePath");

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
            .HasDatabaseName("IX_CourseResult_TenantId");

        builder.HasIndex(e => e.CoursePlanId)
            .HasDatabaseName("IX_CourseResult_CoursePlanId");

        builder.HasIndex(e => e.StaffId)
            .HasDatabaseName("IX_CourseResult_StaffId");

        builder.HasIndex(e => e.ResultStatus)
            .HasDatabaseName("IX_CourseResult_ResultStatus");

        // Unique constraint
        builder.HasIndex(e => new { e.CoursePlanId, e.StaffId, e.TenantId })
            .IsUnique()
            .HasDatabaseName("UQ_CourseResult_Course_Staff");

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
