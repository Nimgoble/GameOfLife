using GameOfLife.Core.Dtos;

namespace GameOfLife.Core.Interfaces;

/// <summary>
/// Persistent storage contract for board states.
/// </summary>
public interface IBoardRepository
{
    /// <summary>Persists a new board and returns it (with generated ID).</summary>
    Task<BoardDto> SaveAsync(BoardDto board, CancellationToken ct = default);

    /// <summary>Returns the board for the given ID, or null if not found.</summary>
    Task<BoardDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Updates an existing board record.</summary>
    Task UpdateAsync(BoardDto board, CancellationToken ct = default);

    /// <summary>Returns all stored boards.</summary>
    Task<IReadOnlyList<BoardDto>> GetAllAsync(CancellationToken ct = default);
}
