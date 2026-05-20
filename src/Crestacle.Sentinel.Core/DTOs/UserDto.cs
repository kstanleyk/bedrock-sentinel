namespace Crestacle.Sentinel.Core.DTOs;

public sealed record UserDto(
    Guid                        Id,
    string                      IdentityId,
    string                      Email,
    string?                     FullName,
    string?                     Phone,
    IEnumerable<RoleSummaryDto> Roles);
