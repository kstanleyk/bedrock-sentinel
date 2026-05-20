using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class MagicLinkTokenConfiguration : IEntityTypeConfiguration<MagicLinkToken>
{
    public void Configure(EntityTypeBuilder<MagicLinkToken> builder)
    {
        builder.ToTable("magic_link_tokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(e => e.UsedAt).HasColumnName("used_at");
        builder.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(64);
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);

        builder.HasIndex(e => e.TokenHash)
            .IsUnique()
            .HasDatabaseName("uq_magic_link_tokens_hash");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_magic_link_tokens_user");
    }
}
