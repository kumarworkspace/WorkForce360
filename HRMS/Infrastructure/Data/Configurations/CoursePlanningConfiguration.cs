using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class CoursePlanningConfiguration : IEntityTypeConfiguration<CoursePlanning>
{
    public void Configure(EntityTypeBuilder<CoursePlanning> builder)
    {
        builder.ToTable("CoursePlanning");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CourseId)
            .IsRequired()
            .HasColumnName("CourseId");

        builder.Property(e => e.StartDate)
            .IsRequired()
            .HasColumnName("StartDate")
            .HasColumnType("date");

        builder.Property(e => e.StartTime)
            .IsRequired()
            .HasColumnName("StartTime")
            .HasColumnType("time(7)");

        builder.Property(e => e.EndDate)
            .IsRequired()
            .HasColumnName("EndDate")
            .HasColumnType("date");

        builder.Property(e => e.EndTime)
            .IsRequired()
            .HasColumnName("EndTime")
            .HasColumnType("time(7)");

        builder.Property(e => e.Venue)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Venue");

        builder.Property(e => e.TrainerId)
            .IsRequired()
            .HasColumnName("TrainerId");

        builder.Property(e => e.Remarks)
            .HasMaxLength(500)
            .HasColumnName("Remarks");

        builder.Property(e => e.UploadFilePaths)
            .HasColumnName("UploadFilePaths")
            .HasColumnType("text");

        builder.Property(e => e.QRCodePath)
            .HasMaxLength(500)
            .HasColumnName("QRCodePath");

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasColumnName("CreatedDate")
            .HasDefaultValueSql("NOW()")
            .HasColumnType("timestamp");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("CreatedBy");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("UpdatedDate")
            .HasColumnType("timestamp");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UpdatedBy");

        // Indexes for better query performance
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("IX_CoursePlanning_TenantId");

        builder.HasIndex(e => e.CourseId)
            .HasDatabaseName("IX_CoursePlanning_CourseId");

        builder.HasIndex(e => e.TrainerId)
            .HasDatabaseName("IX_CoursePlanning_TrainerId");

        builder.HasIndex(e => e.StartDate)
            .HasDatabaseName("IX_CoursePlanning_StartDate");

        // Navigation property relationships
        builder.HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Trainer)
            .WithMany()
            .HasForeignKey(e => e.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
