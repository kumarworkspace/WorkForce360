using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HRMS.Infrastructure.Data.Configurations;

public class HolidayMasterConfiguration : IEntityTypeConfiguration<HolidayMaster>
{
    public void Configure(EntityTypeBuilder<HolidayMaster> builder)
    {
        builder.ToTable("HolidayMaster");

        builder.HasKey(e => e.HolidayId);

        builder.Property(e => e.HolidayId)
            .HasColumnName("HolidayId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.HolidayDate)
            .IsRequired()
            .HasColumnName("HolidayDate")
            .HasColumnType("date");

        builder.Property(e => e.HolidayName)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("HolidayName");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        // CreatedBy column in database is INT, but entity property is string
        // Configure value conversion: INT (database) <-> string (entity)
        // Note: Using helper methods to avoid expression tree limitations with TryParse
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

        // HolidayMaster doesn't have UpdatedBy/UpdatedDate columns in the database
        // Ignore these properties from BaseEntity
        builder.Ignore(e => e.UpdatedBy);
        builder.Ignore(e => e.UpdatedDate);

        // Unique constraint on TenantId and HolidayDate
        builder.HasIndex(e => new { e.TenantId, e.HolidayDate }).IsUnique();
    }

    private static int? ParseIntNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        
        if (int.TryParse(value, out int result))
            return result;
        
        return null;
    }
}

