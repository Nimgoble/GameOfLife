namespace GameOfLife.Core.Interfaces;

/// <summary>
/// Computes Game of Life generation transitions without I/O concerns.
/// Keeping evolution logic behind an interface allows easy unit testing
/// and algorithm substitution.
/// </summary>
public interface IBoardEvolver
{
    /// <summary>Returns the next generation grid for the supplied cells.</summary>
    bool[][] NextGeneration(bool[][] cells);

    /// <summary>Returns a canonical, order-independent hash of a cell grid.</summary>
    string ComputeHash(bool[][] cells);
}
