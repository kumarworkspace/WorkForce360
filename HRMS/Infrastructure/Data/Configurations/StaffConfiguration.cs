using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff");

        builder.HasKey(e => e.StaffId);

        builder.Property(e => e.StaffId)
            .HasColumnName("StaffId")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TenantId)
            .IsRequired()
            .HasColumnName("TenantId");

        builder.Property(e => e.EmployeeCode)
            .HasMaxLength(50)
            .HasColumnName("EmployeeCode");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Name");

        builder.Property(e => e.Company)
            .HasMaxLength(200)
            .HasColumnName("Company");

        builder.Property(e => e.DateOfBirth)
            .HasColumnName("DateOfBirth")
            .HasColumnType("date");

        builder.Property(e => e.GenderId)
            .HasColumnName("GenderId");

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Email");

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(50)
            .HasColumnName("PhoneNumber");

        builder.Property(e => e.Address)
            .HasMaxLength(500)
            .HasColumnName("Address");

        builder.Property(e => e.IdentityCard)
            .HasMaxLength(100)
            .HasColumnName("IdentityCard");

        builder.Property(e => e.Division)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("Division");

        builder.Property(e => e.Department)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("Department");

        builder.Property(e => e.Position)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("Position");

        builder.Property(e => e.EmploymentStatusId)
            .HasColumnName("EmploymentStatusId");

        builder.Property(e => e.DateJoined)
            .HasColumnName("DateJoined")
            .HasColumnType("date");

        builder.Property(e => e.RetirementDate)
            .HasColumnName("RetirementDate")
            .HasColumnType("date");

        builder.Property(e => e.Photo)
            .HasMaxLength(500)
            .HasColumnName("Photo");

        builder.Property(e => e.ReportingManagerId)
            .HasColumnName("ReportingManager");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasColumnName("IsActive")
            .HasDefaultValue(true);

        // CreatedBy column in database is nvarchar(150), matching entity property string
        builder.Property(e => e.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(e => e.CreatedDate)
            .IsRequired()
            .HasColumnName("CreatedDate")
            .HasDefaultValueSql("NOW()")
            .HasColumnType("timestamp");

        // UpdatedBy column in database is nvarchar(150), matching entity property string
        builder.Property(e => e.UpdatedBy)
            .HasColumnName("UpdatedBy")
            .HasMaxLength(150)
            .IsRequired(false);

        builder.Property(e => e.UpdatedDate)
            .HasColumnName("UpdatedDate")
            .HasColumnType("timestamp");

        // Composite unique constraint on TenantId and Email
        builder.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();

        // Relationships
        builder.HasMany(e => e.EducationDetails)
            .WithOne(e => e.Staff)
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ExperienceDetails)
            .WithOne(e => e.Staff)
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.LegalDocuments)
            .WithOne(e => e.Staff)
            .HasForeignKey(e => e.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing relationship for Reporting Manager
        builder.HasOne(e => e.ReportingManager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ReportingManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
