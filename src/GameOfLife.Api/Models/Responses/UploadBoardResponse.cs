namespace GameOfLife.Api.Models.Responses;

/// <summary>Returned after a board is successfully uploaded.</summary>
public sealed class UploadBoardResponse
{
    /// <summary>Unique identifier assigned to the stored board.</summary>
    public Guid Id { get; init; }
}
