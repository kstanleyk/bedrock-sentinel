using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Crestacle.Bedrock.AspNetCore.Middleware;

/// <summary>
/// Enforces that enrollment tokens can only be used on enrollment endpoints.
/// Returns 403 if an enrollment token is presented to a non-enrollment route.
/// Must run after UseAuthentication.
/// </summary>
public sealed class BedrockScopeMiddleware
{
    private readonly RequestDelegate _next;

    public BedrockScopeMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var tokenType = context.User.FindFirstValue("token_type");
        if (tokenType == "enrollment")
        {
            var endpoint = context.GetEndpoint();
            var isEnrollmentRoute = endpoint?.Metadata
                .GetMetadata<EnrollmentEndpointAttribute>() is not null;

            if (!isEnrollmentRoute)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>Marks an endpoint as accessible by enrollment tokens.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class EnrollmentEndpointAttribute : Attribute { }
