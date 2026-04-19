using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LmsCourseConfiguration : IEntityTypeConfiguration<LmsCourse>
{
    public void Configure(EntityTypeBuilder<LmsCourse> builder)
    {
        builder.ToTable("LmsCourses");
        builder.HasKey(e => e.LmsCourseId);
        builder.Property(e => e.LmsCourseId).ValueGeneratedOnAdd();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.ContentUrl).HasMaxLength(500);
        builder.Property(e => e.Tags).HasMaxLength(500);
        builder.Property(e => e.ThumbnailPath).HasMaxLength(500);
        builder.Property(e => e.Objectives).HasColumnType("text");
        builder.Property(e => e.DurationMinutes).HasDefaultValue(0);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedDate).HasColumnType("timestamp");
        builder.HasOne(e => e.CourseType).WithMany().HasForeignKey(e => e.CourseTypeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.Difficulty).WithMany().HasForeignKey(e => e.DifficultyId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.TenantId, e.IsActive });
    }
}
