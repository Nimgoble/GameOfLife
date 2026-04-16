using GameOfLife.Core.Entities;

namespace GameOfLife.Api.Models;

/// <summary>Extension methods that map domain entities to API response models.</summary>
public static class BoardMappingExtensions
{
    public static BoardStateResponse ToResponse(this Board board) => new()
    {
        Id = board.Id,
        Cells = board.Cells,
        Rows = board.Rows,
        Columns = board.Columns,
        CreatedAt = board.CreatedAt
    };

    public static UploadBoardResponse ToUploadResponse(this Guid id) => new() { Id = id };

    public static BoardSummaryResponse ToSummary(this Board board) => new()
    {
        Id = board.Id,
        Rows = board.Rows,
        Columns = board.Columns,
        CreatedAt = board.CreatedAt
    };
}
