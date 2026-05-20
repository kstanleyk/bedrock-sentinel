using Crestacle.Bedrock.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Crestacle.Bedrock.AspNetCore.Extensions;

public static class BedrockApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Bedrock middleware to the pipeline in the required order:
    /// ExceptionMiddleware → UseRouting → UseAuthentication → UseAuthorization → ScopeMiddleware.
    /// </summary>
    public static IApplicationBuilder UseBedrock(this IApplicationBuilder app)
    {
        app.UseMiddleware<BedrockExceptionMiddleware>();
        app.UseRouting();
        app.UseAuthentication();
        app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
        app.UseAuthorization();
        app.UseMiddleware<BedrockScopeMiddleware>();
        return app;
    }
}
