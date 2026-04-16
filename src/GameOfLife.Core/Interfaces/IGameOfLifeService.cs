using GameOfLife.Core.Entities;

namespace GameOfLife.Core.Interfaces;

/// <summary>
/// High-level service contract for Game of Life operations.
/// </summary>
public interface IGameOfLifeService
{
    /// <summary>Stores a board and returns its assigned ID.</summary>
    Task<Guid> UploadBoardAsync(bool[][] cells, CancellationToken ct = default);

    /// <summary>Returns the board state after exactly one generation.</summary>
    Task<Board> GetNextStateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the board state after <paramref name="generations"/> generations.</summary>
    Task<Board> GetStateAfterNGenerationsAsync(Guid id, int generations, CancellationToken ct = default);

    /// <summary>
    /// Returns the final stable (or cycling) state of the board.
    /// Throws <see cref="Exceptions.BoardDidNotStabiliseException"/> if stability
    /// is not reached within the configured maximum iterations.
    /// </summary>
    Task<Board> GetFinalStateAsync(Guid id, CancellationToken ct = default);
}
