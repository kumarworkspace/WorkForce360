using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LegalDocumentConfiguration : IEntityTypeConfiguration<LegalDocument>
{
    public void Configure(EntityTypeBuilder<LegalDocument> builder)
    {
        builder.ToTable("LegalDocuments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.StaffId)
            .IsRequired()
            .HasColumnName("StaffId");

        builder.Property(e => e.DocumentType)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("DocumentType");

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("FileName");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive");

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
    }
}
