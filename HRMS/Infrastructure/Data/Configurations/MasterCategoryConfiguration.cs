using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class MasterCategoryConfiguration : IEntityTypeConfiguration<MasterCategory>
{
    public void Configure(EntityTypeBuilder<MasterCategory> builder)
    {
        builder.ToTable("MasterCategories");
        builder.HasKey(e => e.MasterCategoryId);
        builder.Property(e => e.MasterCategoryId).ValueGeneratedOnAdd();
        builder.Property(e => e.CategoryCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedDate).HasColumnType("timestamp");
        builder.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.TenantId, e.CategoryCode }).IsUnique();
    }
}
