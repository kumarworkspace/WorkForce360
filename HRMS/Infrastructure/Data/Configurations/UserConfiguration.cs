using HRMS.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace HRMS.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.UserId);

        builder.Property(e => e.UserId).HasColumnName("UserId");
        builder.Property(e => e.TenantId).IsRequired().HasColumnName("TenantId");
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(200).HasColumnName("FullName");
        builder.Property(e => e.Email).IsRequired().HasMaxLength(200).HasColumnName("Email");
        builder.Property(e => e.PasswordHash).HasMaxLength(500).HasColumnName("PasswordHash");
        builder.Property(e => e.LoginProvider).HasMaxLength(50).HasColumnName("LoginProvider");
        builder.Property(e => e.Role).IsRequired().HasMaxLength(50).HasColumnName("Role");
        builder.Property(e => e.IsEmailVerified).IsRequired().HasColumnName("IsEmailVerified").HasDefaultValue(false);
        builder.Property(e => e.FailedLoginAttempts).IsRequired().HasColumnName("FailedLoginAttempts").HasDefaultValue(0);
        builder.Property(e => e.IsActive).IsRequired().HasColumnName("IsActive").HasDefaultValue(true);
        builder.Property(e => e.IsLocked).IsRequired().HasColumnName("IsLocked").HasDefaultValue(false);
        builder.Property(e => e.StaffId).HasColumnName("StaffId");
        builder.Property(e => e.CreatedBy).HasColumnName("CreatedBy"); // Database uses int, not string
        builder.Property(e => e.CreatedDate).IsRequired().HasColumnName("CreatedDate").HasDefaultValueSql("NOW()").HasColumnType("timestamp");
        builder.Property(e => e.UpdatedBy).HasColumnName("UpdatedBy"); // Database uses int, not string
        builder.Property(e => e.UpdatedDate).HasColumnName("UpdatedDate").HasColumnType("timestamp");

        // Unique constraints - composite index for multi-tenant support
        builder.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
        
        // Foreign key relationship
        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Note: UserRoles relationship is configured in UserRoleConfiguration to avoid duplicate configuration
    }
}
