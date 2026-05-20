using Crestacle.Bedrock.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crestacle.Bedrock.EntityFramework.Configurations;

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_log");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");

        // UserId is intentionally NOT configured as a foreign key to UserCredentials.
        // Audit rows are append-only and must survive credential deletion — an FK with
        // any cascade behaviour (Cascade, Restrict, SetNull) would either delete audit
        // history silently or block credential removal. Application-level integrity
        // (always writing a valid UserId when one is known) is the sole enforcement.
        // The nullable type supports anonymous events (e.g. failed login for unknown email).
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(64)
            .HasConversion<string>()
            .IsRequired();
        builder.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45).IsRequired();
        builder.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(512).IsRequired();
        builder.Property(e => e.Metadata).HasColumnName("metadata").HasMaxLength(4000);
        builder.Property(e => e.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").HasMaxLength(64);

        builder.HasIndex(e => new { e.UserId, e.OccurredAt })
            .HasDatabaseName("ix_audit_log_user_occurred");

        builder.HasIndex(e => new { e.EventType, e.OccurredAt })
            .HasDatabaseName("ix_audit_log_event_occurred");

        builder.HasIndex(e => new { e.TenantId, e.OccurredAt })
            .HasDatabaseName("ix_audit_log_tenant_occurred");
    }
}
