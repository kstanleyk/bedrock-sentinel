using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class ExternalIdentityConfiguration : IEntityTypeConfiguration<ExternalIdentity>
{
    public void Configure(EntityTypeBuilder<ExternalIdentity> builder)
    {
        builder.ToTable("external_identities");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.Provider).HasColumnName("provider").HasMaxLength(50).IsRequired();
        builder.Property(e => e.ProviderUserId).HasColumnName("provider_user_id").HasMaxLength(256).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);

        builder.HasIndex(e => new { e.Provider, e.ProviderUserId, e.TenantId })
            .IsUnique()
            .HasDatabaseName("uq_external_identities_provider_user_tenant");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_external_identities_user_id");
    }
}
