using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using Microsoft.Extensions.Options;

namespace Crestacle.Bedrock.AspNetCore.Services;

/// <summary>Enforces <see cref="PasswordOptions"/> complexity rules and history-based reuse prevention.</summary>
public sealed class DefaultPasswordValidator : IPasswordValidator
{
    private readonly PasswordOptions _opts;
    private readonly IPasswordHasher _hasher;
    private readonly CommonPasswordValidator _commonValidator;

    public DefaultPasswordValidator(
        IOptions<BedrockOptions> options,
        IPasswordHasher hasher,
        CommonPasswordValidator commonValidator)
    {
        _opts = options.Value.Password;
        _hasher = hasher;
        _commonValidator = commonValidator;
    }

    public bool IsValid(string password, out IReadOnlyList<string> errors)
    {
        var list = new List<string>();

        if (password.Length < _opts.MinLength)
            list.Add($"Password must be at least {_opts.MinLength} characters.");
        if (_opts.RequireUppercase && !password.Any(char.IsUpper))
            list.Add("Password must contain at least one uppercase letter.");
        if (_opts.RequireLowercase && !password.Any(char.IsLower))
            list.Add("Password must contain at least one lowercase letter.");
        if (_opts.RequireDigit && !password.Any(char.IsDigit))
            list.Add("Password must contain at least one digit.");
        if (_opts.RequireSpecialCharacter && !password.Any(c => !char.IsLetterOrDigit(c)))
            list.Add("Password must contain at least one special character.");

        if (!_commonValidator.IsValid(password, out var commonErrors))
            list.AddRange(commonErrors);

        errors = list;
        return list.Count == 0;
    }

    public bool IsPreviouslyUsed(string password, IEnumerable<string> hashes)
    {
        foreach (var hash in hashes)
            if (_hasher.Verify(password, hash))
                return true;
        return false;
    }
}
