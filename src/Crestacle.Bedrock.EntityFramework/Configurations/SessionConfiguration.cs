using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
        builder.Property(e => e.DeviceFingerprint).HasColumnName("device_fingerprint").HasMaxLength(128).IsRequired();
        builder.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45).IsRequired();
        builder.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(512).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.LastActivityAt).HasColumnName("last_activity_at").IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(e => e.RevokedAt).HasColumnName("revoked_at");
        builder.Property(e => e.RevokedByIp).HasColumnName("revoked_by_ip").HasMaxLength(45);
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);
        builder.Property(e => e.AccessTokenJti).HasColumnName("access_token_jti").HasMaxLength(36);
        builder.Property(e => e.AccessTokenExpiresAt).HasColumnName("access_token_expires_at");

        builder.Property(e => e.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        builder.HasIndex(e => e.TokenHash)
            .IsUnique()
            .HasDatabaseName("uq_sessions_token_hash");

        builder.HasIndex(e => new { e.UserId, e.RevokedAt, e.ExpiresAt })
            .HasDatabaseName("ix_sessions_user_active");
    }
}
