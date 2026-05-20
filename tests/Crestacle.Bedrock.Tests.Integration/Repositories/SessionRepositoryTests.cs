using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.EntityFramework;
using Crestacle.Bedrock.EntityFramework.Extensions;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Repositories;

public sealed class SessionRepositoryTests : IDisposable
{
    private readonly TestBedrockContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly ISessionRepository _repo;
    private readonly IBedrockUnitOfWork _uow;

    public SessionRepositoryTests()
    {
        (_context, _connection) = DbContextFactory.Create();

        var services = new ServiceCollection()
            .AddSingleton<BedrockContext>(_context)
            .AddBedrockEntityFramework<BedrockContext>()
            .BuildServiceProvider();

        _repo = services.GetRequiredService<ISessionRepository>();
        _uow = services.GetRequiredService<IBedrockUnitOfWork>();
    }

    private static Session CreateSession(Guid userId, string tokenHash = "hash1")
        => Session.Create(
            userId,
            tokenHash,
            "fp123",
            "127.0.0.1",
            "TestAgent/1.0",
            DateTime.UtcNow.AddDays(7));

    [Fact]
    public async Task AddAsync_ThenGetById_ReturnsSession()
    {
        var userId = Guid.NewGuid();
        var session = CreateSession(userId);
        await _repo.AddAsync(session);
        await _uow.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(session.Id);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByTokenHashAsync_MatchingHash_ReturnsSession()
    {
        var session = CreateSession(Guid.NewGuid(), "unique-hash-xyz");
        await _repo.AddAsync(session);
        await _uow.SaveChangesAsync();

        var result = await _repo.GetByTokenHashAsync("unique-hash-xyz");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetActiveByUserAsync_ActiveSessions_ReturnsThem()
    {
        var userId = Guid.NewGuid();
        var s1 = CreateSession(userId, "h1");
        var s2 = CreateSession(userId, "h2");
        await _repo.AddAsync(s1);
        await _repo.AddAsync(s2);
        await _uow.SaveChangesAsync();

        var sessions = await _repo.GetActiveByUserAsync(userId);

        sessions.Should().HaveCount(2);
    }

    [Fact]
    public async Task CountActiveForUserAsync_ReturnsCorrectCount()
    {
        var userId = Guid.NewGuid();
        await _repo.AddAsync(CreateSession(userId, "cnt-h1"));
        await _repo.AddAsync(CreateSession(userId, "cnt-h2"));
        await _repo.AddAsync(CreateSession(userId, "cnt-h3"));
        await _uow.SaveChangesAsync();

        var count = await _repo.CountActiveForUserAsync(userId);

        count.Should().Be(3);
    }

    [Fact]
    public async Task RevokeAllForUserAsync_RevokesAllActiveSessions()
    {
        var userId = Guid.NewGuid();
        await _repo.AddAsync(CreateSession(userId, "rev-h1"));
        await _repo.AddAsync(CreateSession(userId, "rev-h2"));
        await _uow.SaveChangesAsync();

        await _repo.RevokeAllForUserAsync(userId, "127.0.0.1");

        var count = await _repo.CountActiveForUserAsync(userId);
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetOldestActiveForUserAsync_ReturnsOldestSession()
    {
        var userId = Guid.NewGuid();
        var older = CreateSession(userId, "old-hash");
        await _repo.AddAsync(older);
        await _uow.SaveChangesAsync();

        await Task.Delay(10);

        var newer = CreateSession(userId, "new-hash");
        await _repo.AddAsync(newer);
        await _uow.SaveChangesAsync();

        var oldest = await _repo.GetOldestActiveForUserAsync(userId);

        oldest.Should().NotBeNull();
        oldest!.TokenHash.Should().Be("old-hash");
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
