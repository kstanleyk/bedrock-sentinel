using Crestacle.Bedrock.AspNetCore.Extensions;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using Crestacle.Bedrock.EntityFramework;
using Crestacle.Bedrock.EntityFramework.Caching;
using Crestacle.Bedrock.EntityFramework.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Crestacle.Bedrock.Tests.Integration.Infrastructure;

internal sealed class BedrockTestServer : IDisposable
{
    private const string TestSigningKey = "Bedrock-Integration-Test-Signing-Key-32B!";

    private readonly TestBedrockContext _dbContext;
    private readonly SqliteConnection _connection;
    private readonly IHost _host;

    public HttpClient Client { get; }
    public TestBedrockContext DbContext => _dbContext;
    public IHost Host => _host;

    public BedrockTestServer(
        Action<BedrockOptions>? configureOptions = null,
        Action<IServiceCollection>? configureServices = null)
    {
        (_dbContext, _connection) = DbContextFactory.Create();

        _host = new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.ConfigureServices(services =>
                {
                    services.AddMemoryCache();
                    services.AddSingleton<IBedrockCache, MemoryBedrockCache>();
                    services.AddSingleton<BedrockContext>(_dbContext);
                    services.AddBedrockEntityFramework<BedrockContext>();
                    services.AddBedrockAspNetCore(opts =>
                    {
                        opts.Jwt.SigningKey = TestSigningKey;
                        opts.Jwt.Issuer = "test";
                        opts.Jwt.Audience = "test";
                        opts.Password.MinLength = 12;
                        opts.Lockout.MaxFailedAttempts = 5;
                        opts.Lockout.Duration = TimeSpan.FromSeconds(30);
                        opts.Session.MaxConcurrentSessions = 3;
                        opts.TokenExpiry.EmailVerificationToken = TimeSpan.FromHours(24);
                        configureOptions?.Invoke(opts);
                    })
                    .AddBedrockControllers();

                    configureServices?.Invoke(services);
                    services.AddDataProtection().UseEphemeralDataProtectionProvider();
                });

                web.Configure(app =>
                {
                    app.UseBedrock();
                    app.UseEndpoints(e => e.MapControllers());
                });
            })
            .Build();

        _host.Start();
        Client = _host.GetTestClient();
    }

    public void Dispose()
    {
        Client.Dispose();
        _host.Dispose();
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
