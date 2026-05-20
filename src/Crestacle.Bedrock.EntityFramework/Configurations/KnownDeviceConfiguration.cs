using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class KnownDeviceConfiguration : IEntityTypeConfiguration<KnownDevice>
{
    public void Configure(EntityTypeBuilder<KnownDevice> builder)
    {
        builder.ToTable("known_devices");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.FingerprintHash).HasColumnName("fingerprint_hash").HasMaxLength(128).IsRequired();
        builder.Property(e => e.IpBlock).HasColumnName("ip_block").HasMaxLength(16).IsRequired();
        builder.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(512).IsRequired();
        builder.Property(e => e.FirstSeenAt).HasColumnName("first_seen_at").IsRequired();
        builder.Property(e => e.LastSeenAt).HasColumnName("last_seen_at").IsRequired();
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);

        builder.HasIndex(e => new { e.UserId, e.FingerprintHash })
            .IsUnique()
            .HasDatabaseName("uq_known_devices_user_fingerprint");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_known_devices_user");
    }
}
