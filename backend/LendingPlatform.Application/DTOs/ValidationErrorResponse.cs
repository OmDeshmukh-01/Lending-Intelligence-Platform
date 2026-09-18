namespace LendingPlatform.Application.DTOs;

public sealed class ValidationErrorResponse
{
    public string Type { get; init; } = "ValidationError";
    public IDictionary<string, string[]> Errors { get; init; }
        = new Dictionary<string, string[]>();
}
