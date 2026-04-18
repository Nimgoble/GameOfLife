namespace GameOfLife.Core.Exceptions;

/// <summary>
/// Thrown when a board does not stabilise within the maximum allowed iterations.
/// </summary>
public sealed class BoardDidNotStabiliseException(int maxIterations)
    : Exception($"The board did not reach a stable or cyclic state within {maxIterations} iterations.")
{
    public int MaxIterations { get; } = maxIterations;
}
