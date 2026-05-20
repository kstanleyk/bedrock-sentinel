using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> builder)
    {
        builder.ToTable("consent_records");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.PolicyType).HasColumnName("policy_type").HasMaxLength(50).IsRequired();
        builder.Property(e => e.PolicyVersion).HasColumnName("policy_version").HasMaxLength(20).IsRequired();
        builder.Property(e => e.AcceptedAt).HasColumnName("accepted_at").IsRequired();
        builder.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45).IsRequired();
        builder.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(512).IsRequired();
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);

        builder.HasIndex(e => new { e.UserId, e.AcceptedAt })
            .HasDatabaseName("ix_consent_records_user_accepted");

        builder.HasIndex(e => new { e.TenantId, e.AcceptedAt })
            .HasDatabaseName("ix_consent_records_tenant_accepted");
    }
}
