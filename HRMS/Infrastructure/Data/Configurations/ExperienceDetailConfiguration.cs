using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class ExperienceDetailConfiguration : IEntityTypeConfiguration<ExperienceDetail>
{
    public void Configure(EntityTypeBuilder<ExperienceDetail> builder)
    {
        builder.ToTable("ExperienceDetails");

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

                builder.Property(e => e.StartDate)
                    .IsRequired()
                    .HasColumnName("StartDate")
                    .HasColumnType("timestamp");

                builder.Property(e => e.EndDate)
                    .IsRequired()
                    .HasColumnName("EndDate")
                    .HasColumnType("timestamp");

        builder.Property(e => e.Company)
            .HasMaxLength(150)
            .HasColumnName("Company");

        builder.Property(e => e.Position)
            .HasMaxLength(155)
            .HasColumnName("Position");

        builder.Property(e => e.TotalExperience)
            .HasMaxLength(100)
            .HasColumnName("TotalExperience");

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
