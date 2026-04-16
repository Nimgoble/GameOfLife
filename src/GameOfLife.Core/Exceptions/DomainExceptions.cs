namespace GameOfLife.Core.Exceptions;

/// <summary>Thrown when a requested board cannot be found.</summary>
public sealed class BoardNotFoundException(Guid id)
    : Exception($"Board with ID '{id}' was not found.")
{
    public Guid BoardId { get; } = id;
}

/// <summary>Thrown when board input fails validation.</summary>
public sealed class InvalidBoardException(string message) : Exception(message);

/// <summary>
/// Thrown when a board does not stabilise within the maximum allowed iterations.
/// </summary>
public sealed class BoardDidNotStabiliseException(int maxIterations)
    : Exception($"The board did not reach a stable or cyclic state within {maxIterations} iterations.")
{
    public int MaxIterations { get; } = maxIterations;
}
