using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using Microsoft.Extensions.Options;

namespace Crestacle.Bedrock.AspNetCore.Services;

internal sealed class MfaPolicyService : IMfaPolicyService
{
    private readonly BedrockOptions _options;

    public MfaPolicyService(IOptions<BedrockOptions> options) => _options = options.Value;

    public bool IsMfaRequired(UserCredential credential)
        => _options.Mfa.MandatoryRoles.Count > 0;

    public bool IsInGracePeriod(UserCredential credential)
        => !credential.MfaEnabled
           && credential.MfaGracePeriodEndsAt.HasValue
           && credential.MfaGracePeriodEndsAt.Value > DateTime.UtcNow;

    public bool GracePeriodExpired(UserCredential credential)
        => !credential.MfaEnabled
           && credential.MfaGracePeriodEndsAt.HasValue
           && credential.MfaGracePeriodEndsAt.Value < DateTime.UtcNow;

    public DateTime ComputeGracePeriodEnd()
        => DateTime.UtcNow.AddDays(_options.Mfa.GracePeriodDays);

    public DateTime SetGracePeriod(UserCredential credential)
    {
        var end = ComputeGracePeriodEnd();
        credential.SetGracePeriod(end);
        return end;
    }
}
