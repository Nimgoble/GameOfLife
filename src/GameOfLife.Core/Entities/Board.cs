namespace GameOfLife.Core.Entities;

/// <summary>
/// Represents a stored board state with metadata.
/// </summary>
public sealed class Board
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>The cell grid: true = alive, false = dead.</summary>
    public bool[][] Cells { get; init; } = [];

    public int Rows => Cells.Length;
    public int Columns => Cells.Length > 0 ? Cells[0].Length : 0;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Returns a deep copy of this board with the supplied cell grid.
    /// </summary>
    public Board WithCells(bool[][] cells) => new()
    {
        Id = Id,
        Cells = cells,
        CreatedAt = CreatedAt
    };
}
