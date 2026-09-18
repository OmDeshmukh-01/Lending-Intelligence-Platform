namespace LendingPlatform.Application.DTOs;

public sealed class RuleResultDto
{
    public string Name { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string Detail { get; init; } = string.Empty;
}
