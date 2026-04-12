using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HRMS.Infrastructure.Data.Configurations;

public class LeaveOtRequestConfiguration : IEntityTypeConfiguration<LeaveOtRequest>
{
    public void Configure(EntityTypeBuilder<LeaveOtRequest> builder)
    {
        builder.ToTable("Leave_OT_Request");

        builder.HasKey(e => e.RequestId);

        builder.Property(e => e.RequestId)
            .HasColumnName("RequestId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.StaffId)
            .IsRequired()
            .HasColumnName("StaffId");

        builder.Property(e => e.RequestTypeId)
            .IsRequired()
            .HasColumnName("RequestTypeId");

        builder.Property(e => e.LeaveTypeId)
            .HasColumnName("LeaveTypeId");

        builder.Property(e => e.FromDate)
            .IsRequired()
            .HasColumnName("FromDate")
            .HasColumnType("date");

        builder.Property(e => e.ToDate)
            .IsRequired()
            .HasColumnName("ToDate")
            .HasColumnType("date");

        builder.Property(e => e.TotalDays)
            .HasColumnType("decimal(5,2)")
            .HasColumnName("TotalDays");

        builder.Property(e => e.TotalHours)
            .HasColumnType("decimal(5,2)")
            .HasColumnName("TotalHours");

        builder.Property(e => e.Reason)
            .HasMaxLength(500)
            .HasColumnName("Reason");

        builder.Property(e => e.LeaveStatus)
            .HasColumnName("LeaveStatus");

        builder.Property(e => e.ReportingManagerId)
            .HasColumnName("ReportingManagerId");

        builder.Property(e => e.HRApprovalRequired)
            .IsRequired()
            .HasColumnName("HRApprovalRequired")
            .HasDefaultValue(false);

        builder.Property(e => e.ApprovedBy_L1)
            .HasColumnName("ApprovedBy_L1");

        builder.Property(e => e.ApprovedDate_L1)
            .HasColumnName("ApprovedDate_L1")
            .HasColumnType("timestamp");

        builder.Property(e => e.ApprovedBy_HR)
            .HasColumnName("ApprovedBy_HR");

        builder.Property(e => e.ApprovedDate_HR)
            .HasColumnName("ApprovedDate_HR")
            .HasColumnType("timestamp");

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

        // Relationships
        builder.HasOne(e => e.Staff)
            .WithMany()
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LeaveType)
            .WithMany()
            .HasForeignKey(e => e.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReportingManager)
            .WithMany()
            .HasForeignKey(e => e.ReportingManagerId)
            .OnDelete(DeleteBehavior.Restrict);
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




