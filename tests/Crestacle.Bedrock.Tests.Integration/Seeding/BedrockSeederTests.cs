using Crestacle.Bedrock.EntityFramework;
using Crestacle.Bedrock.EntityFramework.Seeding;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Seeding;

public sealed class BedrockSeederTests : IDisposable
{
    private readonly TestBedrockContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;

    public BedrockSeederTests()
    {
        (_context, _connection) = DbContextFactory.Create();
    }

    [Fact]
    public async Task SeedAsync_BaseImplementation_DoesNotThrow()
    {
        var services = new ServiceCollection()
            .AddSingleton<BedrockContext>(_context)
            .BuildServiceProvider();

        var seeder = new NoOpSeeder();
        var act = async () => await seeder.SeedAsync(services);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SeedAsync_CustomImplementation_IsInvoked()
    {
        var services = new ServiceCollection()
            .AddSingleton<BedrockContext>(_context)
            .BuildServiceProvider();

        var seeder = new RecordingSeeder();
        await seeder.SeedAsync(services);

        seeder.WasInvoked.Should().BeTrue();
    }

    private sealed class NoOpSeeder : BedrockSeeder { }

    private sealed class RecordingSeeder : BedrockSeeder
    {
        public bool WasInvoked { get; private set; }

        public override Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
        {
            WasInvoked = true;
            return Task.CompletedTask;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
