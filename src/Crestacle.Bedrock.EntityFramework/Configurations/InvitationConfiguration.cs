using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("invitations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.Property(e => e.TargetEmail).HasColumnName("target_email").HasMaxLength(256).IsRequired();
        builder.Property(e => e.InvitedByUserId).HasColumnName("invited_by_user_id");
        builder.Property(e => e.RoleHint).HasColumnName("role_hint").HasMaxLength(100);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(e => e.AcceptedAt).HasColumnName("accepted_at");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);

        builder.HasIndex(e => e.TokenHash)
            .IsUnique()
            .HasDatabaseName("uq_invitations_hash");

        builder.HasIndex(e => e.TargetEmail)
            .HasDatabaseName("ix_invitations_email");
    }
}
