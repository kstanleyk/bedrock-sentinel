namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Detects anomalous login patterns by comparing device fingerprints and IP blocks
/// against the user's baseline of known devices.
/// </summary>
public interface IAnomalyDetector
{
    /// <summary>
    /// Computes a device fingerprint as the SHA-256 hex of the concatenated
    /// <paramref name="userAgent"/> and first two IP octets derived from <paramref name="ipAddress"/>.
    /// </summary>
    /// <param name="userAgent">The client User-Agent header string.</param>
    /// <param name="ipAddress">The client IP address (IPv4 or IPv6).</param>
    /// <returns>A lowercase hexadecimal SHA-256 fingerprint string.</returns>
    string ComputeFingerprint(string userAgent, string ipAddress);

    /// <summary>
    /// Extracts the network prefix from an IP address.
    /// For IPv4 returns the first two octets (e.g. <c>"192.168"</c>);
    /// for IPv6 returns the first two groups.
    /// </summary>
    /// <param name="ipAddress">The IP address to extract the network prefix from.</param>
    /// <returns>The network prefix string.</returns>
    string ExtractIpBlock(string ipAddress);

    /// <summary>
    /// Returns <c>true</c> when the combination of <paramref name="fingerprint"/> and
    /// <paramref name="ipBlock"/> is considered anomalous for the given user.
    /// </summary>
    /// <param name="userId">The user to check anomaly status for.</param>
    /// <param name="fingerprint">The computed device fingerprint from <see cref="ComputeFingerprint"/>.</param>
    /// <param name="ipBlock">The network prefix from <see cref="ExtractIpBlock"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> when the login appears anomalous; <c>false</c> when it matches the user's baseline.</returns>
    Task<bool> IsAnomalousAsync(
        Guid userId,
        string fingerprint,
        string ipBlock,
        CancellationToken ct = default);

    /// <summary>
    /// Records the device fingerprint as known for the user and updates the IP-change
    /// tracking window in the cache.
    /// </summary>
    /// <param name="userId">The user to record the known device for.</param>
    /// <param name="fingerprint">The device fingerprint to record.</param>
    /// <param name="ipBlock">The network prefix associated with this login.</param>
    /// <param name="userAgent">The raw User-Agent string to persist with the device record.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordDeviceAsync(
        Guid userId,
        string fingerprint,
        string ipBlock,
        string userAgent,
        CancellationToken ct = default);
}
