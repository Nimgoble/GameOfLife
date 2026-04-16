using System.ComponentModel.DataAnnotations;

namespace GameOfLife.Api.Services;

/// <summary>Strongly-typed configuration for the Game of Life service.</summary>
public sealed class GameOfLifeOptions
{
    public const string SectionName = "GameOfLife";

    /// <summary>
    /// Maximum number of generations evaluated before declaring a board unstable.
    /// Default: 10 000.
    /// </summary>
    [Range(1, 1_000_000)]
    public int MaxStabilisationIterations { get; set; } = 10_000;

    /// <summary>
    /// How many recent generation hashes to keep for cycle detection.
    /// Default: 100 (handles oscillators with long periods).
    /// </summary>
    [Range(2, 10_000)]
    public int CycleDetectionDepth { get; set; } = 100;

    /// <summary>Maximum allowed board dimension (rows or columns).</summary>
    [Range(1, 10_000)]
    public int MaxBoardDimension { get; set; } = 1_000;
}
