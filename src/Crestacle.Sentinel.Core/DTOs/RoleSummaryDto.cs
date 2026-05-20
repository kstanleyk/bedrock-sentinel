namespace Crestacle.Sentinel.Core.DTOs;

public sealed record RoleSummaryDto(
    Guid      Id,
    string    Name,
    string    DisplayName,
    string    Type,
    DateTime? ExpiresOn);
