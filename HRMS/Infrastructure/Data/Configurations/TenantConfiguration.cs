using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(e => e.TenantId);

        builder.Property(e => e.TenantId)
            .HasColumnName("TenantId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CompanyName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("CompanyName");

        builder.Property(e => e.Domain)
            .HasMaxLength(200)
            .HasColumnName("Domain");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        builder.Property(e => e.IsLocked)
            .IsRequired()
            .HasColumnName("IsLocked")
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(150)
            .HasColumnName("CreatedBy");

        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasColumnName("CreatedDate")
            .HasDefaultValueSql("NOW()")
            .HasColumnType("timestamp");

        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(150)
            .HasColumnName("UpdatedBy");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("UpdatedDate")
            .HasColumnType("timestamp");

        // Unique constraint on CompanyName
        builder.HasIndex(e => e.CompanyName).IsUnique();
    }
}


