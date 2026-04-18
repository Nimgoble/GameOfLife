using System.ComponentModel.DataAnnotations;

namespace GameOfLife.Api.Models.Requests;

/// <summary>Query parameters for the "N states ahead" endpoint.</summary>
public sealed class GetNStatesRequest
{
    /// <summary>Number of generations to advance. Must be ≥ 1.</summary>
    [Range(1, 1_000_000, ErrorMessage = "Generations must be between 1 and 1,000,000.")]
    public int Generations { get; set; }
}
