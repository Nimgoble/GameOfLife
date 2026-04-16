using GameOfLife.Core.Entities;

namespace GameOfLife.Core.Interfaces;

/// <summary>
/// Persistent storage contract for board states.
/// </summary>
public interface IBoardRepository
{
    /// <summary>Persists a new board and returns it (with generated ID).</summary>
    Task<Board> SaveAsync(Board board, CancellationToken ct = default);

    /// <summary>Returns the board for the given ID, or null if not found.</summary>
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Updates an existing board record.</summary>
    Task UpdateAsync(Board board, CancellationToken ct = default);

    /// <summary>Returns all stored boards.</summary>
    Task<IReadOnlyList<Board>> GetAllAsync(CancellationToken ct = default);
}
