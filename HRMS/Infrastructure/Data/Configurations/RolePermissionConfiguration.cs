using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.RoleId)
            .IsRequired()
            .HasColumnName("RoleId");

        builder.Property(e => e.PermissionId)
            .IsRequired()
            .HasColumnName("PermissionId");

        builder.Property(e => e.AccessLevel)
            .IsRequired()
            .HasColumnName("AccessLevel");

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedBy)
            .HasColumnName("CreatedBy");

        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasColumnName("CreatedDate")
            .HasDefaultValueSql("NOW()")
            .HasColumnType("timestamp");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UpdatedBy");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("UpdatedDate")
            .HasColumnType("timestamp");

        // Unique constraint on RoleId + PermissionId + TenantId
        builder.HasIndex(e => new { e.RoleId, e.PermissionId, e.TenantId }).IsUnique();

        // Foreign key relationships
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(rp => rp.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}





