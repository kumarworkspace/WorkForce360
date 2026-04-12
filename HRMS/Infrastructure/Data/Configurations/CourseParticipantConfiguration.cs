using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class CourseParticipantConfiguration : IEntityTypeConfiguration<CourseParticipant>
{
    public void Configure(EntityTypeBuilder<CourseParticipant> builder)
    {
        builder.ToTable("CourseParticipant");

        builder.HasKey(e => e.CourseParticipantId);

        builder.Property(e => e.CourseParticipantId)
            .HasColumnName("CourseParticipantId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CoursePlanId)
            .IsRequired()
            .HasColumnName("CoursePlanId");

        builder.Property(e => e.StaffId)
            .IsRequired()
            .HasColumnName("StaffId");

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
            .HasDatabaseName("IX_CourseParticipant_TenantId");

        builder.HasIndex(e => e.CoursePlanId)
            .HasDatabaseName("IX_CourseParticipant_CoursePlanId");

        builder.HasIndex(e => e.StaffId)
            .HasDatabaseName("IX_CourseParticipant_StaffId");

        // Unique constraint
        builder.HasIndex(e => new { e.CoursePlanId, e.StaffId, e.TenantId })
            .IsUnique()
            .HasDatabaseName("UQ_CourseParticipant_Course_Staff");

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
