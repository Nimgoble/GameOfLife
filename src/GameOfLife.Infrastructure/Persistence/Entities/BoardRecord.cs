namespace GameOfLife.Infrastructure.Persistence.Entities;

/// <summary>Flat persistence record for a board.</summary>
public sealed class BoardRecord
{
    public Guid Id { get; set; }

    /// <summary>JSON-serialised 2-D boolean array.</summary>
    public string CellsJson { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }
}
