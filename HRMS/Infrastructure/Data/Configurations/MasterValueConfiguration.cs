using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class MasterValueConfiguration : IEntityTypeConfiguration<MasterValue>
{
    public void Configure(EntityTypeBuilder<MasterValue> builder)
    {
        builder.ToTable("MasterValues");
        builder.HasKey(e => e.MasterValueId);
        builder.Property(e => e.MasterValueId).ValueGeneratedOnAdd();
        builder.Property(e => e.ValueCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.ValueName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.SortOrder).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedDate).HasColumnType("timestamp");
        builder.HasOne(e => e.MasterCategory).WithMany(c => c.MasterValues)
               .HasForeignKey(e => e.MasterCategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.MasterCategoryId, e.ValueCode }).IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.MasterCategoryId });
    }
}
