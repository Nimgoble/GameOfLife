using GameOfLife.Api.Models;
using GameOfLife.Api.Models.Requests;
using GameOfLife.Api.Models.Responses;
using GameOfLife.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.Api.Controllers;

/// <summary>
/// Manages Conway's Game of Life board states.
/// </summary>
[ApiController]
[Route("api/boards")]
[Produces("application/json")]
public sealed class BoardsController
(
    IGameOfLifeService gameOfLifeService,
    BoardValidator validator
) : ControllerBase
{
    // -----------------------------------------------------------------------
    // GET /api/boards
    // -----------------------------------------------------------------------

    /// <summary>Returns a list of stored boards (summary information).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(BoardSummaryResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoards(CancellationToken ct)
    {
        var boards = await gameOfLifeService.ListBoardsAsync(ct);
        var summaries = boards.Select(b => b.ToSummary()).ToArray();
        return Ok(summaries);
    }

    // -----------------------------------------------------------------------
    // POST /api/boards
    // -----------------------------------------------------------------------

    /// <summary>Upload a new board state.</summary>
    /// <remarks>
    /// Accepts a 2-D grid of boolean cells and persists it, returning a unique
    /// identifier that can be used with the other endpoints.
    /// </remarks>
    /// <param name="request">The board payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The assigned board ID.</returns>
    /// <response code="201">Board stored successfully.</response>
    /// <response code="400">The supplied board is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(UploadBoardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadBoard
    (
        [FromBody] UploadBoardRequest request,
        CancellationToken ct
    )
    {
        validator.Validate(request.Cells);

        var id = await gameOfLifeService.UploadBoardAsync(request.Cells, ct);

        return CreatedAtAction
        (
            nameof(GetNextState),
            new { id },
            id.ToUploadResponse()
        );
    }

    // -----------------------------------------------------------------------
    // GET /api/boards/{id}/next
    // -----------------------------------------------------------------------

    /// <summary>Get the next generation of a board.</summary>
    /// <param name="id">The board identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The board state after one generation.</returns>
    /// <response code="200">Next generation computed successfully.</response>
    /// <response code="404">No board exists with the supplied ID.</response>
    [HttpGet("{id:guid}/next")]
    [ProducesResponseType(typeof(BoardStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNextState(Guid id, CancellationToken ct)
    {
        var board = await gameOfLifeService.GetNextStateAsync(id, ct);
        return Ok(board.ToResponse());
    }

    // -----------------------------------------------------------------------
    // GET /api/boards/{id}
    // -----------------------------------------------------------------------

    /// <summary>Get the initially uploaded board state.</summary>
    /// <param name="id">The board identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Board retrieved successfully.</response>
    /// <response code="404">No board exists with the supplied ID.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BoardStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBoard(Guid id, CancellationToken ct)
    {
        var board = await gameOfLifeService.GetBoardAsync(id, ct);
        return Ok(board.ToResponse());
    }

    // -----------------------------------------------------------------------
    // GET /api/boards/{id}/states?generations={n}
    // -----------------------------------------------------------------------

    /// <summary>Get the board state N generations ahead.</summary>
    /// <param name="id">The board identifier.</param>
    /// <param name="request">Query parameters including the number of generations.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The board state after N generations.</returns>
    /// <response code="200">State computed successfully.</response>
    /// <response code="400">Invalid query parameters.</response>
    /// <response code="404">No board exists with the supplied ID.</response>
    [HttpGet("{id:guid}/states")]
    [ProducesResponseType(typeof(BoardStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNStatesAhead
    (
        Guid id,
        [FromQuery] GetNStatesRequest request,
        CancellationToken ct
    )
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var board = await gameOfLifeService.GetStateAfterNGenerationsAsync(id, request.Generations, ct);
        return Ok(board.ToResponse());
    }

    // -----------------------------------------------------------------------
    // GET /api/boards/{id}/final
    // -----------------------------------------------------------------------

    /// <summary>Get the final stable (or cyclic) state of a board.</summary>
    /// <remarks>
    /// Evolves the board until it reaches a state it has already visited
    /// (stable or oscillating) or until the configured maximum iteration
    /// limit is reached, in which case a 422 error is returned.
    /// </remarks>
    /// <param name="id">The board identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The final stable board state.</returns>
    /// <response code="200">Final state determined successfully.</response>
    /// <response code="404">No board exists with the supplied ID.</response>
    /// <response code="422">
    /// The board did not reach a stable or cyclic state within the allowed iterations.
    /// </response>
    [HttpGet("{id:guid}/final")]
    [ProducesResponseType(typeof(BoardStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetFinalState(Guid id, CancellationToken ct)
    {
        var board = await gameOfLifeService.GetFinalStateAsync(id, ct);
        return Ok(board.ToResponse());
    }
}
