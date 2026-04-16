using System.ComponentModel.DataAnnotations;

namespace GameOfLife.Api.Models;

// ---------------------------------------------------------------------------
// Requests
// ---------------------------------------------------------------------------

/// <summary>Payload for uploading a new board state.</summary>
public sealed class UploadBoardRequest
{
    /// <summary>
    /// 2-D grid where <c>true</c> = alive cell, <c>false</c> = dead cell.
    /// All rows must have the same length.
    /// </summary>
    [Required]
    public bool[][] Cells { get; set; } = [];
}

/// <summary>Query parameters for the "N states ahead" endpoint.</summary>
public sealed class GetNStatesRequest
{
    /// <summary>Number of generations to advance. Must be ≥ 1.</summary>
    [Range(1, 1_000_000, ErrorMessage = "Generations must be between 1 and 1,000,000.")]
    public int Generations { get; set; }
}

// ---------------------------------------------------------------------------
// Responses
// ---------------------------------------------------------------------------

/// <summary>Returned after a board is successfully uploaded.</summary>
public sealed class UploadBoardResponse
{
    /// <summary>Unique identifier assigned to the stored board.</summary>
    public Guid Id { get; init; }
}

/// <summary>Represents a board state returned by any query endpoint.</summary>
public sealed class BoardStateResponse
{
    /// <summary>The board ID this state relates to.</summary>
    public Guid Id { get; init; }

    /// <summary>The cell grid at the returned generation.</summary>
    public bool[][] Cells { get; init; } = [];

    /// <summary>Number of rows in the grid.</summary>
    public int Rows { get; init; }

    /// <summary>Number of columns in the grid.</summary>
    public int Columns { get; init; }

    /// <summary>When the board was first uploaded.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>Standard error envelope returned for all non-2xx responses.</summary>
public sealed class ErrorResponse
{
    /// <summary>Short machine-readable error code.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Human-readable description of the error.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Optional field-level validation details.</summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}
