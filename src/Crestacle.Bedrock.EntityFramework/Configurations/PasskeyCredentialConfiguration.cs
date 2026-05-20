using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class PasskeyCredentialConfiguration : IEntityTypeConfiguration<PasskeyCredential>
{
    public void Configure(EntityTypeBuilder<PasskeyCredential> builder)
    {
        builder.ToTable("passkey_credentials");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.CredentialId).HasColumnName("credential_id").HasMaxLength(1024).IsRequired();
        builder.Property(e => e.PublicKeyCose).HasColumnName("public_key_cose").HasMaxLength(2048).IsRequired();
        builder.Property(e => e.SignCount).HasColumnName("sign_count").IsRequired();
        builder.Property(e => e.Transports).HasColumnName("transports").HasMaxLength(128);
        builder.Property(e => e.IsBackedUp).HasColumnName("is_backed_up").IsRequired();
        builder.Property(e => e.FriendlyName).HasColumnName("friendly_name").HasMaxLength(100);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.LastUsedAt).HasColumnName("last_used_at");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);

        builder.HasIndex(e => e.CredentialId)
            .IsUnique()
            .HasDatabaseName("uq_passkey_credentials_credential_id");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_passkey_credentials_user_id");
    }
}
