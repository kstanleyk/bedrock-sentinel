using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ToTable("email_verification_tokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(e => e.UsedAt).HasColumnName("used_at");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(e => e.TokenHash)
            .IsUnique()
            .HasDatabaseName("uq_email_verification_tokens_hash");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_email_verification_tokens_user");
    }
}
