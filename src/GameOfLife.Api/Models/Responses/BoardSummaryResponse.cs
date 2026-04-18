namespace GameOfLife.Api.Models.Responses;

/// <summary>Summary information for a board used in list endpoints.</summary>
public sealed class BoardSummaryResponse
{
    public Guid Id { get; init; }
    public int Rows { get; init; }
    public int Columns { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
