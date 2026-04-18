namespace GameOfLife.Api.Models.Responses;

/// <summary>Represents a board state returned by any query endpoint.</summary>
public sealed class BoardStateResponse
{
    /// <summary>The board ID this state relates to.</summary>
    public Guid Id { get; init; }

    /// <summary>The cell grid at the returned generation.</summary>
    public bool[][] Cells { get; init; } = [];

    /// <summary>Number of rows in the grid.</summary>
    public int Rows { get; init; }

    /// <summary>Number of columns in the grid.</summary>
    public int Columns { get; init; }

    /// <summary>When the board was first uploaded.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
