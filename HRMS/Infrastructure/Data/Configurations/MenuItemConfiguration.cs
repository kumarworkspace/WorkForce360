using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");
        builder.HasKey(e => e.MenuItemId);
        builder.Property(e => e.MenuItemId).ValueGeneratedOnAdd();
        builder.Property(e => e.Label).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Href).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Icon).IsRequired().HasMaxLength(200);
        builder.Property(e => e.PermissionModule).HasMaxLength(150);
        builder.Property(e => e.SortOrder).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnType("timestamp").HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedDate).HasColumnType("timestamp");
        builder.HasOne(e => e.MenuGroup).WithMany(g => g.MenuItems)
               .HasForeignKey(e => e.MenuGroupId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.TenantId, e.MenuGroupId, e.SortOrder });
    }
}
