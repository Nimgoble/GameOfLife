using FluentAssertions;
using GameOfLife.Api.Services;
using GameOfLife.Core.Entities;
using GameOfLife.Core.Exceptions;
using GameOfLife.Core.Interfaces;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace GameOfLife.Tests.Unit;

/// <summary>
/// Tests <see cref="GameOfLifeService"/> orchestration logic with mocked
/// repository and evolver dependencies.
/// </summary>
public sealed class GameOfLifeServiceTests
{
    // -----------------------------------------------------------------------
    // Setup
    // -----------------------------------------------------------------------

    private readonly IBoardRepository _repository = Substitute.For<IBoardRepository>();
    private readonly IBoardEvolver _evolver = Substitute.For<IBoardEvolver>();
    private readonly GameOfLifeService _service;

    private static readonly GameOfLifeOptions DefaultOptions = new()
    {
        MaxStabilisationIterations = 100,
        CycleDetectionDepth = 20,
        MaxBoardDimension = 100
    };

    public GameOfLifeServiceTests()
    {
        var options = Options.Create(DefaultOptions);
        _service = new GameOfLifeService(_repository, _evolver, options);
    }

    private static bool[][] SimpleCells() => [[true, false], [false, true]];
    private static bool[][] EmptyCells() => [[false, false], [false, false]];

    private Board BoardWith(bool[][] cells) => new() { Cells = cells };

    // -----------------------------------------------------------------------
    // UploadBoardAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UploadBoardAsync_SavesAndReturnsId()
    {
        var cells = SimpleCells();
        var stored = BoardWith(cells);
        _repository.SaveAsync(Arg.Any<Board>(), Arg.Any<CancellationToken>())
                   .Returns(stored);

        var id = await _service.UploadBoardAsync(cells);

        id.Should().Be(stored.Id);
        await _repository.Received(1).SaveAsync(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }

    // -----------------------------------------------------------------------
    // GetNextStateAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetNextStateAsync_ReturnsBoardWithEvolvedCells()
    {
        var original = SimpleCells();
        var evolved = EmptyCells();
        var board = BoardWith(original);

        _repository.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        _evolver.NextGeneration(original).Returns(evolved);

        var result = await _service.GetNextStateAsync(board.Id);

        result.Cells.Should().BeEquivalentTo(evolved);
        _evolver.Received(1).NextGeneration(original);
    }

    [Fact]
    public async Task GetNextStateAsync_ThrowsBoardNotFoundException_WhenIdUnknown()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Board?)null);

        await _service.Invoking(s => s.GetNextStateAsync(id))
                      .Should().ThrowAsync<BoardNotFoundException>()
                      .WithMessage($"*{id}*");
    }

    // -----------------------------------------------------------------------
    // GetStateAfterNGenerationsAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetStateAfterNGenerationsAsync_EvolvesExactlyNTimes()
    {
        var cells = SimpleCells();
        var board = BoardWith(cells);
        const int n = 5;

        _repository.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        _evolver.NextGeneration(Arg.Any<bool[][]>()).Returns(cells); // idempotent mock

        await _service.GetStateAfterNGenerationsAsync(board.Id, n);

        _evolver.Received(n).NextGeneration(Arg.Any<bool[][]>());
    }

    [Fact]
    public async Task GetStateAfterNGenerationsAsync_ThrowsBoardNotFoundException_WhenIdUnknown()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Board?)null);

        await _service.Invoking(s => s.GetStateAfterNGenerationsAsync(id, 3))
                      .Should().ThrowAsync<BoardNotFoundException>();
    }

    // -----------------------------------------------------------------------
    // GetFinalStateAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetFinalStateAsync_ReturnsState_WhenBoardStabilisesImmediately()
    {
        // If the evolver returns a grid we've already seen, we should detect stability.
        var stable = EmptyCells();
        var board = BoardWith(stable);

        _repository.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);

        // First call: evolver returns same stable state as input
        _evolver.NextGeneration(Arg.Any<bool[][]>()).Returns(stable);

        // Hashes: first call returns "HASH_A", second call (for evolved) also "HASH_A"
        _evolver.ComputeHash(Arg.Any<bool[][]>())
                .Returns("HASH_A", "HASH_A");

        var result = await _service.GetFinalStateAsync(board.Id);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFinalStateAsync_ThrowsBoardDidNotStabilise_WhenLimitExceeded()
    {
        var cells = SimpleCells();
        var board = BoardWith(cells);

        _repository.GetByIdAsync(board.Id, Arg.Any<CancellationToken>()).Returns(board);
        _evolver.NextGeneration(Arg.Any<bool[][]>()).Returns(cells);

        // Always return a unique hash so we never detect a cycle
        int call = 0;
        _evolver.ComputeHash(Arg.Any<bool[][]>()).Returns(_ => $"HASH_{call++}");

        await _service.Invoking(s => s.GetFinalStateAsync(board.Id))
                      .Should().ThrowAsync<BoardDidNotStabiliseException>()
                      .WithMessage($"*{DefaultOptions.MaxStabilisationIterations}*");
    }

    [Fact]
    public async Task GetFinalStateAsync_ThrowsBoardNotFoundException_WhenIdUnknown()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Board?)null);

        await _service.Invoking(s => s.GetFinalStateAsync(id))
                      .Should().ThrowAsync<BoardNotFoundException>();
    }
}
