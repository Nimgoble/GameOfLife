using System.ComponentModel.DataAnnotations;

namespace GameOfLife.Api.Models.Requests;

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
