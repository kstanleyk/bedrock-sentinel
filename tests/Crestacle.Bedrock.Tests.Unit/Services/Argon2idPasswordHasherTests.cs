using Crestacle.Bedrock.AspNetCore.Services;
using FluentAssertions;
using Xunit;

namespace Crestacle.Bedrock.Tests.Unit.Services;

public sealed class Argon2idPasswordHasherTests
{
    private readonly Argon2idPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesArgon2idPrefixedString()
    {
        var hash = _hasher.Hash("Password1!");
        hash.Should().StartWith("argon2id$");
    }

    [Fact]
    public void Hash_TwoCallsSamePasword_ProduceDifferentHashes()
    {
        var h1 = _hasher.Hash("Password1!");
        var h2 = _hasher.Hash("Password1!");
        h1.Should().NotBe(h2);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("Password1!");
        _hasher.Verify("Password1!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("Password1!");
        _hasher.Verify("WrongPassword1!", hash).Should().BeFalse();
    }

    [Fact]
    public void NeedsRehash_Argon2idHash_ReturnsFalse()
    {
        var hash = _hasher.Hash("Password1!");
        _hasher.NeedsRehash(hash).Should().BeFalse();
    }

    [Fact]
    public void NeedsRehash_Pbkdf2Hash_ReturnsTrue()
    {
        // ASP.NET Core Identity v3 PBKDF2-SHA256 hash of "Password1!"
        const string pbkdf2Hash = "AQAAAAIAAYagAAAAENBKnvCHHF9hJHlVv0Y+hRQOWAFP0kYV2GHHfT/B9kFE9bM7hLdNZvAVH3KD5u8P";
        _hasher.NeedsRehash(pbkdf2Hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_UnknownHashFormat_ReturnsFalse()
    {
        _hasher.Verify("Password1!", "not-a-valid-hash-format").Should().BeFalse();
    }
}
