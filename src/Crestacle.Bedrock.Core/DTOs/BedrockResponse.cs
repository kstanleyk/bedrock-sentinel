namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>Standard response envelope for all Bedrock HTTP endpoints.</summary>
public sealed record BedrockResponse<T>
{
    public bool Success { get; init; }

    /// <summary>Machine-readable error code when <see cref="Success"/> is <c>false</c>. See <c>BedrockErrorCodes</c>.</summary>
    public string? Code { get; init; }

    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static BedrockResponse<T> Ok(T data)
        => new() { Success = true, Data = data };

    public static BedrockResponse<T> Fail(params string[] errors)
        => new() { Success = false, Errors = errors };

    public static BedrockResponse<T> Fail(IEnumerable<string> errors)
        => new() { Success = false, Errors = [.. errors] };

    public static BedrockResponse<T> Fail(string code, string message)
        => new() { Success = false, Code = code, Errors = [message] };
}

/// <summary>Non-generic variant for void/no-data responses.</summary>
public sealed record BedrockResponse
{
    public bool Success { get; init; }

    /// <summary>Machine-readable error code when <see cref="Success"/> is <c>false</c>. See <c>BedrockErrorCodes</c>.</summary>
    public string? Code { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = [];

    public static BedrockResponse Ok()
        => new() { Success = true };

    public static BedrockResponse Fail(params string[] errors)
        => new() { Success = false, Errors = errors };

    public static BedrockResponse Fail(string code, string message)
        => new() { Success = false, Code = code, Errors = [message] };
}
