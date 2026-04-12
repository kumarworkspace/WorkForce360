using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HRMS.Core.Domain.Entities;

namespace HRMS.Infrastructure.Data.Configurations;

public class SSOConfiguration : IEntityTypeConfiguration<SSO>
{
    public void Configure(EntityTypeBuilder<SSO> builder)
    {
        builder.ToTable("tbl_SSO");

        builder.HasKey(e => e.SSOId);

        builder.Property(e => e.TenantId).IsRequired().HasColumnName("TenantId");
        builder.Property(e => e.Provider).IsRequired().HasMaxLength(100);
        builder.Property(e => e.UserId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Username).HasMaxLength(150);
        builder.Property(e => e.Email).HasMaxLength(200);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.CreatedBy).HasMaxLength(150);
        builder.Property(e => e.CreatedDate).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(150);
    }
}
