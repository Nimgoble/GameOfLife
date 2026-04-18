using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GameOfLife.Api.Models.Requests;
using GameOfLife.Api.Models.Responses;
using Xunit;

namespace GameOfLife.Tests.Integration;

/// <summary>
/// End-to-end integration tests that exercise the full HTTP pipeline
/// (routing → controller → service → in-memory EF Core repository).
/// </summary>
public sealed class BoardsControllerIntegrationTests(GameOfLifeWebAppFactory factory)
    : IClassFixture<GameOfLifeWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // -----------------------------------------------------------------------
    // Helper boards
    // -----------------------------------------------------------------------

    /// <summary>2x2 block — a Conway still-life that never changes.</summary>
    private static bool[][] BlockCells() => [[true, true], [true, true]];

    /// <summary>Blinker in a 3x3 grid — a period-2 oscillator.</summary>
    private static bool[][] BlinkerCells() =>
    [
        [false, false, false],
        [true,  true,  true],
        [false, false, false]
    ];

    /// <summary>Single live cell — dies immediately (0 neighbours).</summary>
    private static bool[][] SingleCellCells() => [[true]];

    // -----------------------------------------------------------------------
    // POST /api/boards
    // -----------------------------------------------------------------------

    [Fact]
    public async Task PostBoard_ValidPayload_Returns201WithId()
    {
        var response = await PostBoardAsync(BlockCells());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<UploadBoardResponse>();
        body!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task PostBoard_EmptyGrid_Returns400()
    {
        var payload = new UploadBoardRequest { Cells = [] };
        var response = await _client.PostAsJsonAsync("/api/boards", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostBoard_JaggedGrid_Returns400()
    {
        var payload = new UploadBoardRequest
        {
            Cells = [[true, false], [true]] // row lengths differ
        };
        var response = await _client.PostAsJsonAsync("/api/boards", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // -----------------------------------------------------------------------
    // GET /api/boards/{id}/next
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetNextState_ExistingBoard_Returns200WithEvolvedCells()
    {
        var id = await UploadAndGetIdAsync(BlinkerCells());

        var response = await _client.GetAsync($"/api/boards/{id}/next");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BoardStateResponse>();
        body!.Id.Should().Be(id);
        body.Rows.Should().Be(3);
        body.Columns.Should().Be(3);

        // Blinker should have rotated to vertical
        body.Cells[0][1].Should().BeTrue("top-centre alive after blinker step");
        body.Cells[1][1].Should().BeTrue("centre alive");
        body.Cells[2][1].Should().BeTrue("bottom-centre alive");
        body.Cells[1][0].Should().BeFalse("left dead");
        body.Cells[1][2].Should().BeFalse("right dead");
    }

    [Fact]
    public async Task GetNextState_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/boards/{Guid.NewGuid()}/next");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // GET /api/boards/{id}/states?generations={n}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetNStates_Blinker2Generations_ReturnsOriginalState()
    {
        var id = await UploadAndGetIdAsync(BlinkerCells());

        var response = await _client.GetAsync($"/api/boards/{id}/states?generations=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BoardStateResponse>();

        // Blinker is period-2, so 2 steps returns to the original horizontal orientation
        body!.Cells[1][0].Should().BeTrue();
        body.Cells[1][1].Should().BeTrue();
        body.Cells[1][2].Should().BeTrue();
        body.Cells[0][1].Should().BeFalse();
        body.Cells[2][1].Should().BeFalse();
    }

    [Fact]
    public async Task GetNStates_GenerationsZero_Returns400()
    {
        var id = await UploadAndGetIdAsync(BlockCells());
        var response = await _client.GetAsync($"/api/boards/{id}/states?generations=0");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetNStates_UnknownId_Returns404()
    {
        var response = await _client.GetAsync(
            $"/api/boards/{Guid.NewGuid()}/states?generations=1");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // GET /api/boards/{id}/final
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetFinalState_Block_ReturnsUnchangedGrid()
    {
        var id = await UploadAndGetIdAsync(BlockCells());

        var response = await _client.GetAsync($"/api/boards/{id}/final");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BoardStateResponse>();

        // Block is a still life; every cell should equal the input
        body!.Cells[0][0].Should().BeTrue();
        body.Cells[0][1].Should().BeTrue();
        body.Cells[1][0].Should().BeTrue();
        body.Cells[1][1].Should().BeTrue();
    }

    [Fact]
    public async Task GetFinalState_SingleCell_ReturnsDead()
    {
        // A single live cell dies immediately; the all-dead grid is the final state.
        var id = await UploadAndGetIdAsync(SingleCellCells());

        var response = await _client.GetAsync($"/api/boards/{id}/final");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BoardStateResponse>();
        body!.Cells[0][0].Should().BeFalse("single cell should die");
    }

    [Fact]
    public async Task GetFinalState_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/boards/{Guid.NewGuid()}/final");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // -----------------------------------------------------------------------
    // Response shape
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetNextState_ResponseIncludesCorrectDimensions()
    {
        // 3x5 board
        var cells = Enumerable.Range(0, 3)
                              .Select(_ => new bool[5])
                              .ToArray();

        var id = await UploadAndGetIdAsync(cells);
        var response = await _client.GetFromJsonAsync<BoardStateResponse>(
            $"/api/boards/{id}/next");

        response!.Rows.Should().Be(3);
        response.Columns.Should().Be(5);
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private async Task<HttpResponseMessage> PostBoardAsync(bool[][] cells)
    {
        var payload = new UploadBoardRequest { Cells = cells };
        return await _client.PostAsJsonAsync("/api/boards", payload);
    }

    private async Task<Guid> UploadAndGetIdAsync(bool[][] cells)
    {
        var response = await PostBoardAsync(cells);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<UploadBoardResponse>();
        return body!.Id;
    }
}
