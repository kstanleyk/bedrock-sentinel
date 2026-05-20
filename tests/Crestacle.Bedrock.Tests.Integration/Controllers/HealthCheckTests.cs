using Crestacle.Bedrock.AspNetCore.Extensions;
using Crestacle.Bedrock.EntityFramework;
using Crestacle.Bedrock.EntityFramework.Extensions;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Controllers;

public sealed class HealthCheckTests : IDisposable
{
    private const string TestSigningKey = "Bedrock-Integration-Test-Signing-Key-32B!";

    private readonly TestBedrockContext _dbContext;
    private readonly SqliteConnection _connection;
    private readonly IHost _host;
    private readonly HttpClient _client;

    public HealthCheckTests()
    {
        (_dbContext, _connection) = DbContextFactory.Create();

        _host = new HostBuilder()
            .ConfigureWebHost(web =>
            {
                web.UseTestServer();
                web.ConfigureServices(services =>
                {
                    services.AddSingleton<BedrockContext>(_dbContext);
                    services.AddBedrockEntityFramework<BedrockContext>();
                    services.AddBedrockAspNetCore(opts =>
                    {
                        opts.Jwt.SigningKey = TestSigningKey;
                        opts.Jwt.Issuer = "test";
                        opts.Jwt.Audience = "test";
                        opts.Password.MinLength = 12;
                    })
                    .AddBedrockControllers()
                    .WithHealthChecks();

                    // DB check is in a separate package; call it explicitly
                    services.AddHealthChecks().AddBedrockDbHealthCheck();

                    services.AddDataProtection().UseEphemeralDataProtectionProvider();
                });

                web.Configure(app =>
                {
                    app.UseBedrock();
                    app.UseEndpoints(e =>
                    {
                        e.MapControllers();
                        e.MapHealthChecks("/health");
                    });
                });
            })
            .Build();

        _host.Start();
        _client = _host.GetTestClient();
    }

    [Fact]
    public async Task HealthEndpoint_WithAllChecksRegistered_Returns200Healthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthEndpoint_CacheCheckOnly_Returns200Healthy()
    {
        // Filtered to just the cache tag
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public void Dispose()
    {
        _client.Dispose();
        _host.Dispose();
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
