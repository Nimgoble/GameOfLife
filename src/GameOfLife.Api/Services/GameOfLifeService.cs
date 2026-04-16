using GameOfLife.Core.Entities;
using GameOfLife.Core.Exceptions;
using GameOfLife.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace GameOfLife.Api.Services;

/// <summary>
/// Orchestrates board persistence and evolution logic.
/// </summary>
public sealed class GameOfLifeService
(
    IBoardRepository repository,
    IBoardEvolver evolver,
    IOptions<GameOfLifeOptions> options
) : IGameOfLifeService
{
    private readonly GameOfLifeOptions _options = options.Value;

    public async Task<Guid> UploadBoardAsync(bool[][] cells, CancellationToken ct = default)
    {
        var board = new Board { Cells = cells };
        var saved = await repository.SaveAsync(board, ct);
        return saved.Id;
    }

    public async Task<Board> GetNextStateAsync(Guid id, CancellationToken ct = default)
    {
        var board = await RequireBoardAsync(id, ct);
        var nextCells = evolver.NextGeneration(board.Cells);
        return board.WithCells(nextCells);
    }

    public async Task<Board> GetStateAfterNGenerationsAsync(
        Guid id, int generations, CancellationToken ct = default)
    {
        var board = await RequireBoardAsync(id, ct);
        var cells = board.Cells;

        for (int i = 0; i < generations; i++)
            cells = evolver.NextGeneration(cells);

        return board.WithCells(cells);
    }

    public async Task<Board> GetFinalStateAsync(Guid id, CancellationToken ct = default)
    {
        var board = await RequireBoardAsync(id, ct);
        var cells = board.Cells;

        // We detect both stable states and cycles by keeping a history of hashes.
        // A cycle window of _options.CycleDetectionDepth generations is checked;
        // if the same hash reappears the board is considered "final".
        var seenHashes = new Dictionary<string, int>();
        int maxIterations = _options.MaxStabilisationIterations;

        for (int i = 0; i < maxIterations; i++)
        {
            string hash = evolver.ComputeHash(cells);

            if (seenHashes.TryGetValue(hash, out _))
            {
                // State has repeated – board is in a stable or cyclic pattern.
                return board.WithCells(cells);
            }

            seenHashes[hash] = i;

            // Prune the history window to bound memory on very long runs.
            if (seenHashes.Count > _options.CycleDetectionDepth)
            {
                var oldest = seenHashes.MinBy(kvp => kvp.Value).Key;
                seenHashes.Remove(oldest);
            }

            cells = evolver.NextGeneration(cells);
        }

        throw new BoardDidNotStabiliseException(maxIterations);
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private async Task<Board> RequireBoardAsync(Guid id, CancellationToken ct)
    {
        var board = await repository.GetByIdAsync(id, ct);
        return board ?? throw new BoardNotFoundException(id);
    }
}
