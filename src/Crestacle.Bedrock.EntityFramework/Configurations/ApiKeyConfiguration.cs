using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.KeyHash).HasColumnName("key_hash").HasMaxLength(64).IsRequired();
        builder.Property(e => e.Prefix).HasColumnName("prefix").HasMaxLength(8).IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.LastUsedAt).HasColumnName("last_used_at");
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        builder.Property(e => e.RevokedAt).HasColumnName("revoked_at");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);

        builder.Ignore(e => e.IsActive);

        builder.HasIndex(e => e.KeyHash)
            .IsUnique()
            .HasDatabaseName("uq_api_keys_hash");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_api_keys_user");
    }
}
