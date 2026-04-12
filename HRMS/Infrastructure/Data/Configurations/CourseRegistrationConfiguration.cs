using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class CourseRegistrationConfiguration : IEntityTypeConfiguration<CourseRegistration>
{
    public void Configure(EntityTypeBuilder<CourseRegistration> builder)
    {
        builder.ToTable("CourseRegistration");

        builder.HasKey(e => e.CourseId);

        builder.Property(e => e.CourseId)
            .HasColumnName("CourseId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("Code");

        builder.Property(e => e.CourseCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("CourseCode");

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Title");

        builder.Property(e => e.TrainingModule)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("TrainingModule");

        builder.Property(e => e.CourseTypeId)
            .IsRequired()
            .HasColumnName("CourseTypeId");

        builder.Property(e => e.CourseCategoryId)
            .IsRequired()
            .HasColumnName("CourseCategoryId");

        builder.Property(e => e.TrainerId)
            .IsRequired()
            .HasColumnName("TrainerId");

        builder.Property(e => e.Duration)
            .IsRequired()
            .HasColumnName("Duration")
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.ValidityPeriod)
            .IsRequired()
            .HasColumnName("ValidityPeriod");

        builder.Property(e => e.UploadFilePath)
            .HasMaxLength(500)
            .HasColumnName("UploadFilePath");

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

        // Composite unique constraint on TenantId and Code
        builder.HasIndex(e => new { e.TenantId, e.Code })
            .IsUnique()
            .HasDatabaseName("IX_CourseRegistration_TenantId_Code");

        // Navigation property relationships
        builder.HasOne(e => e.CourseType)
            .WithMany()
            .HasForeignKey(e => e.CourseTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CourseCategory)
            .WithMany()
            .HasForeignKey(e => e.CourseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ValidityPeriodType)
            .WithMany()
            .HasForeignKey(e => e.ValidityPeriod)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Trainer)
            .WithMany()
            .HasForeignKey(e => e.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
