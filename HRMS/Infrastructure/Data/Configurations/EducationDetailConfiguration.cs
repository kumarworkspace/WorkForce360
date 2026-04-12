using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class EducationDetailConfiguration : IEntityTypeConfiguration<EducationDetail>
{
    public void Configure(EntityTypeBuilder<EducationDetail> builder)
    {
        builder.ToTable("EducationDetails");

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

        builder.Property(e => e.Institution)
            .HasMaxLength(250)
            .HasColumnName("Institution");

        builder.Property(e => e.Qualification)
            .HasMaxLength(250)
            .HasColumnName("Qualification");

        builder.Property(e => e.YearOfPassing)
            .HasColumnName("YearOfPassing");

        builder.Property(e => e.GradeOrPercentage)
            .HasMaxLength(50)
            .HasColumnName("GradeOrPercentage");

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
