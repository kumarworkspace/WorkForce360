using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class ProgressTrackingConfiguration : IEntityTypeConfiguration<ProgressTracking>
{
    public void Configure(EntityTypeBuilder<ProgressTracking> builder)
    {
        builder.ToTable("ProgressTracking");
        builder.HasKey(e => e.ProgressTrackingId);
        builder.Property(e => e.ProgressTrackingId).ValueGeneratedOnAdd();
        builder.Property(e => e.ProgressPct).HasColumnType("numeric(5,2)").HasDefaultValue(0);
        builder.Property(e => e.LastAccessedDate).HasColumnType("timestamp");
        builder.Property(e => e.IsCompleted).HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedDate).HasColumnType("timestamp");
        builder.HasOne(e => e.Enrollment).WithMany(en => en.Progress)
               .HasForeignKey(e => e.EnrollmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Module).WithMany(m => m.ProgressRecords)
               .HasForeignKey(e => e.LmsModuleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.EnrollmentId, e.LmsModuleId }).IsUnique();
    }
}
