using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.EntityFramework;
using Crestacle.Bedrock.EntityFramework.Extensions;
using Crestacle.Bedrock.Tests.Integration.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Crestacle.Bedrock.Tests.Integration.Repositories;

public sealed class CredentialRepositoryTests : IDisposable
{
    private readonly TestBedrockContext _context;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
    private readonly ICredentialRepository _repo;
    private readonly IBedrockUnitOfWork _uow;

    public CredentialRepositoryTests()
    {
        (_context, _connection) = DbContextFactory.Create();

        var services = new ServiceCollection()
            .AddSingleton<BedrockContext>(_context)
            .AddBedrockEntityFramework<BedrockContext>()
            .BuildServiceProvider();

        _repo = services.GetRequiredService<ICredentialRepository>();
        _uow = services.GetRequiredService<IBedrockUnitOfWork>();
    }

    [Fact]
    public async Task AddAsync_ThenGetByUserId_ReturnsCredential()
    {
        var userId = Guid.NewGuid();
        var credential = UserCredential.Create(userId, "user@example.com", "hash");
        await _repo.AddAsync(credential);
        await _uow.SaveChangesAsync();

        var result = await _repo.GetByUserIdAsync(userId);

        result.Should().NotBeNull();
        result!.Email.Should().Be("user@example.com");
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsCredential()
    {
        var credential = UserCredential.Create(Guid.NewGuid(), "find@example.com", "hash");
        await _repo.AddAsync(credential);
        await _uow.SaveChangesAsync();

        var result = await _repo.GetByEmailAsync("find@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("find@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_UnknownEmail_ReturnsNull()
    {
        var result = await _repo.GetByEmailAsync("unknown@example.com");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_AfterAdd_ReturnsTrue()
    {
        var credential = UserCredential.Create(Guid.NewGuid(), "exists@example.com", "hash");
        await _repo.AddAsync(credential);
        await _uow.SaveChangesAsync();

        var exists = await _repo.ExistsByEmailAsync("exists@example.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_UnknownEmail_ReturnsFalse()
    {
        var exists = await _repo.ExistsByEmailAsync("nobody@example.com");

        exists.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
