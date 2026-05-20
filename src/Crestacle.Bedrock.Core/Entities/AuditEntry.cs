using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Append-only record of every security event. Never updated or deleted.
/// Written atomically in the same unit of work as the mutation that triggered it.
/// </summary>
public sealed class AuditEntry
{
    private AuditEntry() { }

    public Guid Id { get; private set; }

    /// <summary>Null for anonymous events such as a failed login with an unknown email.</summary>
    public Guid? UserId { get; private set; }

    public AuditEventType EventType { get; private set; }

    /// <summary>Client IP address; max 45 chars.</summary>
    public string IpAddress { get; private set; } = string.Empty;

    /// <summary>Client User-Agent header; max 512 chars.</summary>
    public string UserAgent { get; private set; } = string.Empty;

    /// <summary>Optional JSON-serialized structured context for the event.</summary>
    public string? Metadata { get; private set; }

    public DateTime OccurredAt { get; private set; }
    public string? TenantId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static AuditEntry Create(
        AuditEventType eventType,
        string ipAddress,
        string userAgent,
        Guid? userId = null,
        string? metadata = null,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

        return new AuditEntry
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            UserId = userId,
            Metadata = metadata,
            TenantId = tenantId,
            OccurredAt = DateTime.UtcNow
        };
    }
}
