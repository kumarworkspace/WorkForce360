using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HRMS.Infrastructure.Data.Configurations;

public class LeaveTypeMasterConfiguration : IEntityTypeConfiguration<LeaveTypeMaster>
{
    public void Configure(EntityTypeBuilder<LeaveTypeMaster> builder)
    {
        builder.ToTable("LeaveTypeMaster");

        builder.HasKey(e => e.LeaveTypeId);

        builder.Property(e => e.LeaveTypeId)
            .HasColumnName("LeaveTypeId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.LeaveTypeName)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("LeaveTypeName");

        builder.Property(e => e.MaxDaysPerYear)
            .IsRequired()
            .HasColumnType("decimal(5,2)")
            .HasColumnName("MaxDaysPerYear");

        builder.Property(e => e.IsPaid)
            .IsRequired()
            .HasColumnName("IsPaid")
            .HasDefaultValue(true);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        // CreatedBy column in database is INT, but entity property is string
        // Configure value conversion: INT (database) <-> string (entity)
        builder.Property(e => e.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasConversion(
                new ValueConverter<string?, int?>(
                    v => ParseIntNullable(v),
                    v => v.HasValue ? v.Value.ToString() : null))
            .HasColumnType("int");

        builder.Property(e => e.CreatedDate)
            .HasColumnName("CreatedDate")
            .HasColumnType("timestamp");

        // UpdatedBy column in database is INT, but entity property is string
        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UpdatedBy")
            .HasConversion(
                new ValueConverter<string?, int?>(
                    v => ParseIntNullable(v),
                    v => v.HasValue ? v.Value.ToString() : null))
            .HasColumnType("int");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("UpdatedDate")
            .HasColumnType("timestamp");

        // Unique constraint on TenantId and LeaveTypeName
        builder.HasIndex(e => new { e.TenantId, e.LeaveTypeName }).IsUnique();
    }

    private static int? ParseIntNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        
        if (int.TryParse(value, out var intValue))
            return intValue;
        
        return null;
    }
}




