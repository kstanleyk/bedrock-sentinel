using Crestacle.Bedrock.AspNetCore;
using Crestacle.Bedrock.AspNetCore.Services;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Crestacle.Bedrock.HaveIBeenPwned;

/// <summary>Fluent extensions for wiring the Have I Been Pwned password validator into a Bedrock builder.</summary>
public static class HaveIBeenPwnedExtensions
{
    /// <summary>
    /// Replaces the default <see cref="IPasswordValidator"/> with <see cref="HaveIBeenPwnedPasswordValidator"/>,
    /// which wraps the default complexity checks and additionally rejects passwords found in known data breaches.
    /// </summary>
    /// <remarks>
    /// The HIBP API is called with k-anonymity (only the first 5 SHA-1 hex characters are sent).
    /// The validator is fail-open: if the API is unreachable, the password is accepted.
    /// </remarks>
    public static IBedrockBuilder WithHaveIBeenPwnedPasswordValidator(this IBedrockBuilder builder)
    {
        builder.Services.AddHttpClient("hibp", client =>
        {
            client.BaseAddress = new Uri("https://api.pwnedpasswords.com");
            client.Timeout = TimeSpan.FromSeconds(3);
            client.DefaultRequestHeaders.Add("User-Agent", "Crestacle.Bedrock");
        });

        var descriptor = builder.Services
            .FirstOrDefault(d => d.ServiceType == typeof(IPasswordValidator));
        if (descriptor is not null)
            builder.Services.Remove(descriptor);

        // Register DefaultPasswordValidator as its concrete type so it can be injected as the
        // inner (complexity + history) validator without causing a circular IPasswordValidator loop.
        builder.Services.TryAddScoped<DefaultPasswordValidator>();

        builder.Services.AddScoped<IPasswordValidator>(sp =>
            new HaveIBeenPwnedPasswordValidator(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<DefaultPasswordValidator>()));

        return builder;
    }
}
