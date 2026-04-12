using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class MasterDropdownConfiguration : IEntityTypeConfiguration<MasterDropdown>
{
    public void Configure(EntityTypeBuilder<MasterDropdown> builder)
    {
        builder.ToTable("tbl_Master_Dropdown");

        builder.HasKey(e => e.Id);

        // Id column is INT in database (identity column), entity property is int - no conversion needed
        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Category");

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("Code");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Name");

        builder.Property(e => e.Description)
            .HasMaxLength(255)
            .HasColumnName("Description");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        // CreatedBy column in database is NVARCHAR, entity property is string
        builder.Property(e => e.CreatedBy)
            .HasColumnName("CreatedBy")
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasColumnName("CreatedDate")
            .HasDefaultValueSql("NOW()")
            .HasColumnType("timestamp");

        // UpdatedBy column in database is NVARCHAR, entity property is string
        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UpdatedBy")
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("UpdatedDate")
            .IsRequired(false)
            .HasColumnType("timestamp");
    }
}
