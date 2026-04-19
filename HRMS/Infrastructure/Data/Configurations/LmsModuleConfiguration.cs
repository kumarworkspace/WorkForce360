using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LmsModuleConfiguration : IEntityTypeConfiguration<LmsModule>
{
    public void Configure(EntityTypeBuilder<LmsModule> builder)
    {
        builder.ToTable("LmsModules");
        builder.HasKey(e => e.LmsModuleId);
        builder.Property(e => e.LmsModuleId).ValueGeneratedOnAdd();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.ContentUrl).HasMaxLength(500);
        builder.Property(e => e.ContentType).HasMaxLength(50);
        builder.Property(e => e.DurationMinutes).HasDefaultValue(0);
        builder.Property(e => e.SortOrder).HasDefaultValue(0);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedDate).HasColumnType("timestamp");
        builder.HasOne(e => e.Course).WithMany(c => c.Modules)
               .HasForeignKey(e => e.LmsCourseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.LmsCourseId, e.SortOrder });
    }
}
