using GameOfLife.Api.Models.Responses;
using GameOfLife.Core.Dtos;

namespace GameOfLife.Api.Models;

/// <summary>Extension methods that map domain entities to API response models.</summary>
public static class BoardMappingExtensions
{
    public static BoardStateResponse ToResponse(this BoardDto board) => new()
    {
        Id = board.Id,
        Cells = board.Cells,
        Rows = board.Rows,
        Columns = board.Columns,
        CreatedAt = board.CreatedAt
    };

    public static UploadBoardResponse ToUploadResponse(this Guid id) => new() { Id = id };

    public static BoardSummaryResponse ToSummary(this BoardDto board) => new()
    {
        Id = board.Id,
        Rows = board.Rows,
        Columns = board.Columns,
        CreatedAt = board.CreatedAt
    };
}
