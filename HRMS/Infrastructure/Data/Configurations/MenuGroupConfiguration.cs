using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class MenuGroupConfiguration : IEntityTypeConfiguration<MenuGroup>
{
    public void Configure(EntityTypeBuilder<MenuGroup> builder)
    {
        builder.ToTable("MenuGroups");
        builder.HasKey(e => e.MenuGroupId);
        builder.Property(e => e.MenuGroupId).ValueGeneratedOnAdd();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Icon).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PermissionModule).HasMaxLength(150);
        builder.Property(e => e.SortOrder).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedDate).HasColumnType("timestamp");
        builder.HasOne<Tenant>().WithMany().HasForeignKey(e => e.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.TenantId, e.SortOrder });
    }
}
