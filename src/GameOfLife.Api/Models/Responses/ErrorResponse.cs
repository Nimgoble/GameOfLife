namespace GameOfLife.Api.Models.Responses;

/// <summary>Standard error envelope returned for all non-2xx responses.</summary>
public sealed class ErrorResponse
{
    /// <summary>Short machine-readable error code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Human-readable description of the error.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Optional field-level validation details.</summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}
