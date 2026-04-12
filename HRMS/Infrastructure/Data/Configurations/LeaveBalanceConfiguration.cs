using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalance");

        builder.HasKey(e => e.LeaveBalanceId);

        builder.Property(e => e.LeaveBalanceId)
            .HasColumnName("LeaveBalanceId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.StaffId)
            .IsRequired()
            .HasColumnName("StaffId");

        builder.Property(e => e.LeaveTypeId)
            .IsRequired()
            .HasColumnName("LeaveTypeId");

        builder.Property(e => e.TotalDays)
            .IsRequired()
            .HasColumnName("TotalDays")
            .HasColumnType("DECIMAL(5,2)");

        builder.Property(e => e.UsedDays)
            .IsRequired()
            .HasColumnName("UsedDays")
            .HasColumnType("DECIMAL(5,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.RemainingDays)
            .IsRequired()
            .HasColumnName("RemainingDays")
            .HasColumnType("DECIMAL(5,2)")
            .HasComputedColumnSql("\"TotalDays\" - \"UsedDays\"", stored: true);

        builder.Property(e => e.Year)
            .IsRequired()
            .HasColumnName("Year");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedDate)
            .HasColumnName("CreatedDate")
            .HasColumnType("timestamp");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("CreatedBy");

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("UpdatedDate")
            .HasColumnType("timestamp");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UpdatedBy");

        // Relationships
        builder.HasOne(e => e.Staff)
            .WithMany()
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.LeaveType)
            .WithMany()
            .HasForeignKey(e => e.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: one balance per staff, leave type, and year
        builder.HasIndex(e => new { e.TenantId, e.StaffId, e.LeaveTypeId, e.Year }).IsUnique();
    }
}





