using System.Text.Json;
using GameOfLife.Core.Entities;
using GameOfLife.Core.Interfaces;
using GameOfLife.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IBoardRepository"/>.
/// </summary>
public sealed class BoardRepository(GameOfLifeDbContext db) : IBoardRepository
{
    public async Task<Board> SaveAsync(Board board, CancellationToken ct = default)
    {
        var record = ToRecord(board);
        await db.Boards.AddAsync(record, ct);
        await db.SaveChangesAsync(ct);
        return board;
    }

    public async Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await db.Boards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return record is null ? null : ToBoard(record);
    }

    public async Task UpdateAsync(Board board, CancellationToken ct = default)
    {
        var record = await db.Boards.FindAsync([board.Id], ct);
        if (record is null) return;

        record.CellsJson = SerializeCells(board.Cells);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Board>> GetAllAsync(CancellationToken ct = default)
    {
        var records = await db.Boards
            .AsNoTracking()
            .ToListAsync(ct);

        return records.Select(ToBoard).ToList();
    }

    // -----------------------------------------------------------------------
    // Mapping helpers
    // -----------------------------------------------------------------------

    private static BoardRecord ToRecord(Board board) => new()
    {
        Id = board.Id,
        CellsJson = SerializeCells(board.Cells),
        CreatedAt = board.CreatedAt
    };

    private static Board ToBoard(BoardRecord record) => new()
    {
        Id = record.Id,
        Cells = DeserializeCells(record.CellsJson),
        CreatedAt = record.CreatedAt
    };

    private static string SerializeCells(bool[][] cells) =>
        JsonSerializer.Serialize(cells);

    private static bool[][] DeserializeCells(string json) =>
        JsonSerializer.Deserialize<bool[][]>(json) ?? [];
}
