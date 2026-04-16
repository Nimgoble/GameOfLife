using GameOfLife.Core.Exceptions;
using GameOfLife.Api.Services;
using Microsoft.Extensions.Options;

namespace GameOfLife.Api.Models;

/// <summary>
/// Validates raw board input before it reaches the domain layer.
/// </summary>
public sealed class BoardValidator(IOptions<GameOfLifeOptions> options)
{
    private readonly GameOfLifeOptions _options = options.Value;

    /// <summary>
    /// Validates that the supplied cells form a well-formed, non-empty rectangular grid
    /// within the configured size limits.
    /// </summary>
    /// <exception cref="InvalidBoardException">Thrown when any rule is violated.</exception>
    public void Validate(bool[][] cells)
    {
        if (cells is null || cells.Length == 0)
            throw new InvalidBoardException("Board must contain at least one row.");

        int cols = cells[0].Length;

        if (cols == 0)
            throw new InvalidBoardException("Board rows must contain at least one cell.");

        if (cells.Length > _options.MaxBoardDimension)
            throw new InvalidBoardException($"Board exceeds the maximum allowed row count of {_options.MaxBoardDimension}.");

        if (cols > _options.MaxBoardDimension)
            throw new InvalidBoardException($"Board exceeds the maximum allowed column count of {_options.MaxBoardDimension}.");

        for (int i = 1; i < cells.Length; i++)
        {
            if (cells[i].Length != cols)
            {
                var message = $"All rows must have the same length. Row 0 has {cols} cells but row {i} has {cells[i].Length}.";
                throw new InvalidBoardException(message);
            }
        }
    }
}
