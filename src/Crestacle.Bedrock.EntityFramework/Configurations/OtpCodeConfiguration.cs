using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("otp_codes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.Purpose)
            .HasColumnName("purpose")
            .HasMaxLength(32)
            .HasConversion<string>()
            .IsRequired();
        builder.Property(e => e.CodeHash).HasColumnName("code_hash").HasMaxLength(128).IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(e => e.UsedAt).HasColumnName("used_at");
        builder.Property(e => e.InvalidatedAt).HasColumnName("invalidated_at");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(e => new { e.UserId, e.Purpose, e.UsedAt, e.InvalidatedAt, e.ExpiresAt })
            .HasDatabaseName("ix_otp_codes_user_purpose_active");
    }
}
