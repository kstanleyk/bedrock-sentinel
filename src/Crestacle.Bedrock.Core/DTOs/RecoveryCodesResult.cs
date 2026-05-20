namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>
/// Returned when recovery codes are generated or regenerated.
/// The <see cref="Codes"/> list contains plaintext codes shown exactly once;
/// hashed versions are stored in the database.
/// </summary>
public sealed record RecoveryCodesResult(IReadOnlyList<string> Codes);
