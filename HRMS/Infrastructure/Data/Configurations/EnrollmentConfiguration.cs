using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");
        builder.HasKey(e => e.EnrollmentId);
        builder.Property(e => e.EnrollmentId).ValueGeneratedOnAdd();
        builder.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Enrolled");
        builder.Property(e => e.EnrolledDate).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("NOW()");
        builder.Property(e => e.CompletedDate).HasColumnType("timestamp");
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedDate).HasColumnType("timestamp");
        builder.HasOne(e => e.Staff).WithMany().HasForeignKey(e => e.StaffId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Course).WithMany(c => c.Enrollments)
               .HasForeignKey(e => e.LmsCourseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.StaffId, e.LmsCourseId, e.TenantId }).IsUnique();
        builder.HasIndex(e => new { e.StaffId, e.TenantId });
    }
}
