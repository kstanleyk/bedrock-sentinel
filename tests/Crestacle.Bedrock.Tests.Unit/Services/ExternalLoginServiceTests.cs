using Crestacle.Bedrock.AspNetCore.Services;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Core.Exceptions;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.Core.Interfaces.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Crestacle.Bedrock.Tests.Unit.Services;

public sealed class ExternalLoginServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private const string Provider = "testprovider";
    private const string ProviderUserId = "ext-user-123";
    private const string ValidToken = "valid-token";
    private const string Email = "user@example.com";

    private readonly IExternalIdentityValidator _validator = Substitute.For<IExternalIdentityValidator>();
    private readonly IExternalIdentityRepository _externalRepo = Substitute.For<IExternalIdentityRepository>();
    private readonly ICredentialRepository _credentialRepo = Substitute.For<ICredentialRepository>();
    private readonly IAuditRepository _auditRepo = Substitute.For<IAuditRepository>();
    private readonly IRefreshTokenService _refreshTokens = Substitute.For<IRefreshTokenService>();
    private readonly IBedrockUnitOfWork _unitOfWork = Substitute.For<IBedrockUnitOfWork>();

    private ExternalLoginService Build() => new(
        new[] { _validator },
        _externalRepo,
        _credentialRepo,
        _auditRepo,
        _refreshTokens,
        _unitOfWork,
        NullLogger<ExternalLoginService>.Instance);

    private static UserCredential MakeCredential(bool hasPassword = true)
    {
        var credential = UserCredential.Create(UserId, Email, "initial-hash");
        credential.ConfirmEmail();
        if (!hasPassword)
            credential.Anonymize();
        return credential;
    }

    private static TokenPair MakeTokenPair() =>
        new("access", "refresh", DateTime.UtcNow.AddHours(1));

    // -------------------------------------------------------------------------
    // ExternalLoginAsync — happy path: existing ExternalIdentity
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExternalLogin_LinkedIdentityExists_ReturnsTokens()
    {
        _validator.ProviderName.Returns(Provider);
        _validator.ValidateAsync(ValidToken, Arg.Any<CancellationToken>())
            .Returns(new ExternalIdentityClaims(ProviderUserId, Email, null));

        var externalId = ExternalIdentity.Create(UserId, Provider, ProviderUserId);
        _externalRepo.GetByProviderAsync(Provider, ProviderUserId, Arg.Any<CancellationToken>())
            .Returns(externalId);

        var credential = MakeCredential();
        _credentialRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(credential);

        _refreshTokens.IssueAsync(
            UserId, Email, Arg.Any<IEnumerable<string>>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(MakeTokenPair());

        var svc = Build();
        var result = await svc.ExternalLoginAsync(Provider, ValidToken, "127.0.0.1", "ua");

        result.Tokens.Should().NotBeNull();
        result.Tokens!.AccessToken.Should().Be("access");
    }

    // -------------------------------------------------------------------------
    // ExternalLoginAsync — auto-link via email
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExternalLogin_NoLinkedIdentityButEmailMatches_AutoLinksAndReturnsTokens()
    {
        _validator.ProviderName.Returns(Provider);
        _validator.ValidateAsync(ValidToken, Arg.Any<CancellationToken>())
            .Returns(new ExternalIdentityClaims(ProviderUserId, Email, null));

        _externalRepo.GetByProviderAsync(Provider, ProviderUserId, Arg.Any<CancellationToken>())
            .Returns((ExternalIdentity?)null);

        var credential = MakeCredential();
        _credentialRepo.GetByEmailAsync(Email, Arg.Any<CancellationToken>())
            .Returns(credential);

        _refreshTokens.IssueAsync(
            UserId, Email, Arg.Any<IEnumerable<string>>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(MakeTokenPair());

        var svc = Build();
        var result = await svc.ExternalLoginAsync(Provider, ValidToken, "127.0.0.1", "ua");

        result.Tokens.Should().NotBeNull();
        await _externalRepo.Received(1).AddAsync(
            Arg.Is<ExternalIdentity>(e => e.UserId == UserId && e.Provider == Provider),
            Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // ExternalLoginAsync — unknown provider
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExternalLogin_UnknownProvider_Throws()
    {
        _validator.ProviderName.Returns("other");

        var svc = Build();
        await svc.Invoking(s =>
                s.ExternalLoginAsync("unknown-provider", ValidToken, "127.0.0.1", "ua"))
            .Should().ThrowAsync<BedrockValidationException>()
            .WithMessage("*unsupported provider*");
    }

    // -------------------------------------------------------------------------
    // ExternalLoginAsync — invalid token
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExternalLogin_InvalidToken_Throws()
    {
        _validator.ProviderName.Returns(Provider);
        _validator.ValidateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ExternalIdentityClaims?)null);

        var svc = Build();
        await svc.Invoking(s =>
                s.ExternalLoginAsync(Provider, "bad-token", "127.0.0.1", "ua"))
            .Should().ThrowAsync<BedrockValidationException>()
            .WithMessage("*could not be validated*");
    }

    // -------------------------------------------------------------------------
    // ExternalLoginAsync — no account found
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExternalLogin_NoAccountResolvable_ThrowsNotFound()
    {
        _validator.ProviderName.Returns(Provider);
        _validator.ValidateAsync(ValidToken, Arg.Any<CancellationToken>())
            .Returns(new ExternalIdentityClaims(ProviderUserId, Email: null, DisplayName: null));

        _externalRepo.GetByProviderAsync(Provider, ProviderUserId, Arg.Any<CancellationToken>())
            .Returns((ExternalIdentity?)null);

        var svc = Build();
        await svc.Invoking(s =>
                s.ExternalLoginAsync(Provider, ValidToken, "127.0.0.1", "ua"))
            .Should().ThrowAsync<BedrockNotFoundException>();
    }

    // -------------------------------------------------------------------------
    // LinkExternalIdentityAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Link_ValidToken_AddsIdentityAndAudits()
    {
        _validator.ProviderName.Returns(Provider);
        _validator.ValidateAsync(ValidToken, Arg.Any<CancellationToken>())
            .Returns(new ExternalIdentityClaims(ProviderUserId, Email, null));

        _externalRepo.GetByProviderAsync(Provider, ProviderUserId, Arg.Any<CancellationToken>())
            .Returns((ExternalIdentity?)null);

        _credentialRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(MakeCredential());

        var svc = Build();
        await svc.LinkExternalIdentityAsync(UserId, Provider, ValidToken);

        await _externalRepo.Received(1).AddAsync(
            Arg.Is<ExternalIdentity>(e => e.UserId == UserId && e.Provider == Provider),
            Arg.Any<CancellationToken>());
        await _auditRepo.Received(1).AddAsync(
            Arg.Is<AuditEntry>(a => a.EventType == AuditEventType.ExternalIdentityLinked),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Link_AlreadyLinked_Throws()
    {
        _validator.ProviderName.Returns(Provider);
        _validator.ValidateAsync(ValidToken, Arg.Any<CancellationToken>())
            .Returns(new ExternalIdentityClaims(ProviderUserId, Email, null));

        var existing = ExternalIdentity.Create(UserId, Provider, ProviderUserId);
        _externalRepo.GetByProviderAsync(Provider, ProviderUserId, Arg.Any<CancellationToken>())
            .Returns(existing);

        var svc = Build();
        await svc.Invoking(s => s.LinkExternalIdentityAsync(UserId, Provider, ValidToken))
            .Should().ThrowAsync<BedrockValidationException>()
            .WithMessage("*already linked*");
    }

    // -------------------------------------------------------------------------
    // UnlinkExternalIdentityAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Unlink_HasPassword_Succeeds()
    {
        var identity = ExternalIdentity.Create(UserId, Provider, ProviderUserId);
        _externalRepo.GetForUserAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(new List<ExternalIdentity> { identity });

        _credentialRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(MakeCredential(hasPassword: true));

        var svc = Build();
        await svc.UnlinkExternalIdentityAsync(UserId, Provider);

        await _externalRepo.Received(1).DeleteAsync(identity, Arg.Any<CancellationToken>());
        await _auditRepo.Received(1).AddAsync(
            Arg.Is<AuditEntry>(a => a.EventType == AuditEventType.ExternalIdentityUnlinked),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unlink_NoPasswordNoOtherIdentity_Throws()
    {
        var identity = ExternalIdentity.Create(UserId, Provider, ProviderUserId);
        _externalRepo.GetForUserAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(new List<ExternalIdentity> { identity });

        _credentialRepo.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(MakeCredential(hasPassword: false));

        var svc = Build();
        await svc.Invoking(s => s.UnlinkExternalIdentityAsync(UserId, Provider))
            .Should().ThrowAsync<BedrockValidationException>()
            .WithMessage("*only credential*");
    }

    [Fact]
    public async Task Unlink_NotLinked_ThrowsNotFound()
    {
        _externalRepo.GetForUserAsync(UserId, Arg.Any<CancellationToken>())
            .Returns(new List<ExternalIdentity>());

        var svc = Build();
        await svc.Invoking(s => s.UnlinkExternalIdentityAsync(UserId, Provider))
            .Should().ThrowAsync<BedrockNotFoundException>();
    }
}
