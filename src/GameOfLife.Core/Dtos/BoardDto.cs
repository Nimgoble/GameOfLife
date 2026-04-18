namespace GameOfLife.Core.Dtos;

/// <summary>
/// Represents a stored board state with metadata.
/// </summary>
public sealed class BoardDto
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
    public BoardDto WithCells(bool[][] cells) => new()
    {
        Id = Id,
        Cells = cells,
        CreatedAt = CreatedAt
    };
}
