namespace Crestacle.Bedrock.Core.Options;

/// <summary>Anomaly detection settings for login monitoring.</summary>
public sealed class AnomalyDetectionOptions
{
    /// <summary>Enable or disable anomaly detection globally. Default: true.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Time window within which an IP block change is considered a rapid change and
    /// triggers a step-up challenge. Default: 10 minutes.
    /// </summary>
    public TimeSpan RapidIpChangeWindow { get; set; } = TimeSpan.FromMinutes(10);
}
