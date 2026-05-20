using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Interfaces.Services;

namespace Crestacle.Bedrock.Tests.Integration.Infrastructure;

/// <summary>
/// Test validator that treats the token itself as the provider user ID.
/// If the token starts with "email:", that suffix is used as the email claim too.
/// </summary>
internal sealed class FakeExternalIdentityValidator : IExternalIdentityValidator
{
    public string ProviderName => "fake";

    public Task<ExternalIdentityClaims?> ValidateAsync(string providerToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(providerToken))
            return Task.FromResult<ExternalIdentityClaims?>(null);

        if (providerToken.StartsWith("email:", StringComparison.OrdinalIgnoreCase))
        {
            var email = providerToken["email:".Length..];
            return Task.FromResult<ExternalIdentityClaims?>(
                new ExternalIdentityClaims(providerToken, email, null));
        }

        return Task.FromResult<ExternalIdentityClaims?>(
            new ExternalIdentityClaims(providerToken, Email: null, DisplayName: null));
    }
}
