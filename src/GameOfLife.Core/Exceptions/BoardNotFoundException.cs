namespace GameOfLife.Core.Exceptions;

/// <summary>Thrown when a requested board cannot be found.</summary>
public sealed class BoardNotFoundException(Guid id)
    : Exception($"Board with ID '{id}' was not found.")
{
    public Guid BoardId { get; } = id;
}
