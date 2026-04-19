using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HRMS.Core.Domain.Entities;

namespace HRMS.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(e => e.AuditId);

        builder.Property(e => e.AuditId).HasColumnName("AuditId");
        builder.Property(e => e.TenantId).IsRequired().HasColumnName("TenantId");
        builder.Property(e => e.UserId).HasColumnName("UserId");

        // Convert enum to string for database
        builder.Property(e => e.ActionType).IsRequired().HasMaxLength(100).HasColumnName("ActionType")
            .HasConversion<string>();

        builder.Property(e => e.Module).HasMaxLength(200).HasColumnName("Module");
        builder.Property(e => e.RecordId).HasColumnName("RecordId");
        builder.Property(e => e.Description).HasColumnName("Description");
        builder.Property(e => e.IPAddress).HasMaxLength(100).HasColumnName("IPAddress");
        builder.Property(e => e.IsActive).IsRequired().HasColumnName("IsActive").HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasColumnName("CreatedBy");
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnName("CreatedDate").HasDefaultValueSql("NOW()").HasColumnType("timestamp");
        builder.Property(e => e.UpdatedBy).HasColumnName("UpdatedBy");
        builder.Property(e => e.UpdatedDate).HasColumnName("UpdatedDate").HasColumnType("timestamp");
    }
}
