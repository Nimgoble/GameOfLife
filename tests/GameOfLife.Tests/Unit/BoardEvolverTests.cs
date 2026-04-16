using FluentAssertions;
using GameOfLife.Api.Services;
using Xunit;

namespace GameOfLife.Tests.Unit;

/// <summary>
/// Verifies that <see cref="BoardEvolver"/> applies Conway's rules correctly
/// against well-known patterns.
/// </summary>
public sealed class BoardEvolverTests
{
    private readonly BoardEvolver _evolver = new();

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>Converts a jagged array of 0/1 ints to a bool[][] grid.</summary>
    private static bool[][] Grid(params int[][] rows) =>
        rows.Select(r => r.Select(c => c == 1).ToArray()).ToArray();

    // -----------------------------------------------------------------------
    // Empty / edge cases
    // -----------------------------------------------------------------------

    [Fact]
    public void NextGeneration_EmptyGrid_ReturnsEmptyGrid()
    {
        var result = _evolver.NextGeneration([]);
        result.Should().BeEmpty();
    }

    [Fact]
    public void NextGeneration_SingleDeadCell_StaysDead()
    {
        var input = Grid([0]);
        var result = _evolver.NextGeneration(input);
        result[0][0].Should().BeFalse();
    }

    [Fact]
    public void NextGeneration_SingleLiveCell_Dies()
    {
        var input = Grid([1]);
        var result = _evolver.NextGeneration(input);
        result[0][0].Should().BeFalse("a lone live cell has no neighbours and dies");
    }

    // -----------------------------------------------------------------------
    // Still lifes (stable patterns)
    // -----------------------------------------------------------------------

    [Fact]
    public void NextGeneration_Block_IsStillLife()
    {
        // 2x2 block — the canonical still life
        var block = Grid(
            [1, 1],
            [1, 1]);

        var result = _evolver.NextGeneration(block);

        result[0][0].Should().BeTrue();
        result[0][1].Should().BeTrue();
        result[1][0].Should().BeTrue();
        result[1][1].Should().BeTrue();
    }

    [Fact]
    public void NextGeneration_Beehive_IsStillLife()
    {
        // 3x4 beehive
        var beehive = Grid(
            [0, 1, 1, 0],
            [1, 0, 0, 1],
            [0, 1, 1, 0]);

        var result = _evolver.NextGeneration(beehive);

        // The beehive must be identical to the input.
        for (int r = 0; r < beehive.Length; r++)
            for (int c = 0; c < beehive[r].Length; c++)
                result[r][c].Should().Be(beehive[r][c],
                    $"beehive cell [{r},{c}] should be unchanged");
    }

    // -----------------------------------------------------------------------
    // Oscillators
    // -----------------------------------------------------------------------

    [Fact]
    public void NextGeneration_Blinker_OscillatesPeriod2()
    {
        // Horizontal blinker in a 3x3 grid
        var horizontal = Grid(
            [0, 0, 0],
            [1, 1, 1],
            [0, 0, 0]);

        var vertical = _evolver.NextGeneration(horizontal);

        // After 1 generation: vertical blinker
        vertical[0][1].Should().BeTrue("top-centre should be alive");
        vertical[1][1].Should().BeTrue("centre should stay alive");
        vertical[2][1].Should().BeTrue("bottom-centre should be alive");
        vertical[1][0].Should().BeFalse("left should be dead");
        vertical[1][2].Should().BeFalse("right should be dead");

        // After 2 generations: back to horizontal
        var backToHorizontal = _evolver.NextGeneration(vertical);
        backToHorizontal[1][0].Should().BeTrue();
        backToHorizontal[1][1].Should().BeTrue();
        backToHorizontal[1][2].Should().BeTrue();
        backToHorizontal[0][1].Should().BeFalse();
        backToHorizontal[2][1].Should().BeFalse();
    }

    [Fact]
    public void NextGeneration_Toad_OscillatesPeriod2()
    {
        // Toad oscillator (2 generations should return to original)
        var gen0 = Grid(
            [0, 0, 0, 0],
            [0, 1, 1, 1],
            [1, 1, 1, 0],
            [0, 0, 0, 0]);

        var gen1 = _evolver.NextGeneration(gen0);
        var gen2 = _evolver.NextGeneration(gen1);

        for (int r = 0; r < gen0.Length; r++)
            for (int c = 0; c < gen0[r].Length; c++)
                gen2[r][c].Should().Be(gen0[r][c],
                    $"toad should be back to gen0 at [{r},{c}] after 2 generations");
    }

    // -----------------------------------------------------------------------
    // Spaceships
    // -----------------------------------------------------------------------

    [Fact]
    public void NextGeneration_Glider_MovesCorrectly()
    {
        // Glider in a 6x6 grid (enough room for one step)
        var gen0 = Grid(
            [0, 1, 0, 0, 0, 0],
            [0, 0, 1, 0, 0, 0],
            [1, 1, 1, 0, 0, 0],
            [0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0]);

        var gen1 = _evolver.NextGeneration(gen0);

        // Known gen1 of glider
        var expected = Grid(
            [0, 0, 0, 0, 0, 0],
            [1, 0, 1, 0, 0, 0],
            [0, 1, 1, 0, 0, 0],
            [0, 1, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0]);

        for (int r = 0; r < expected.Length; r++)
            for (int c = 0; c < expected[r].Length; c++)
                gen1[r][c].Should().Be(expected[r][c],
                    $"glider gen1 mismatch at [{r},{c}]");
    }

    // -----------------------------------------------------------------------
    // Survival / birth rules
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0, false)] // 0 neighbours -> dies
    [InlineData(1, false)] // 1 neighbour  -> dies
    [InlineData(2, true)]  // 2 neighbours -> survives
    [InlineData(3, true)]  // 3 neighbours -> survives
    [InlineData(4, false)] // 4 neighbours -> dies (overcrowding)
    [InlineData(5, false)]
    [InlineData(6, false)]
    [InlineData(7, false)]
    [InlineData(8, false)]
    public void SurvivalRule_LiveCellWithNNeighbours(int neighbours, bool expectedAlive)
    {
        // Build a 5x5 grid with the centre cell alive and 'neighbours' live neighbours
        // arranged along the top row (safe in a sparse grid context).
        var cells = CreateGridWithLiveCentreAndNeighbours(neighbours);
        var result = _evolver.NextGeneration(cells);
        result[2][2].Should().Be(expectedAlive,
            $"live cell with {neighbours} neighbours should {(expectedAlive ? "survive" : "die")}");
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]  // exactly 3 -> born
    [InlineData(4, false)]
    [InlineData(5, false)]
    [InlineData(6, false)]
    [InlineData(7, false)]
    [InlineData(8, false)]
    public void BirthRule_DeadCellWithNNeighbours(int neighbours, bool expectedAlive)
    {
        var cells = CreateGridWithDeadCentreAndNeighbours(neighbours);
        var result = _evolver.NextGeneration(cells);
        result[2][2].Should().Be(expectedAlive,
            $"dead cell with {neighbours} neighbours should {(expectedAlive ? "be born" : "stay dead")}");
    }

    // -----------------------------------------------------------------------
    // Hash
    // -----------------------------------------------------------------------

    [Fact]
    public void ComputeHash_SameGrids_ReturnSameHash()
    {
        var a = Grid([1, 0], [0, 1]);
        var b = Grid([1, 0], [0, 1]);
        _evolver.ComputeHash(a).Should().Be(_evolver.ComputeHash(b));
    }

    [Fact]
    public void ComputeHash_DifferentGrids_ReturnDifferentHash()
    {
        var a = Grid([1, 0], [0, 1]);
        var b = Grid([0, 1], [1, 0]);
        _evolver.ComputeHash(a).Should().NotBe(_evolver.ComputeHash(b));
    }

    // -----------------------------------------------------------------------
    // Grid construction helpers
    // -----------------------------------------------------------------------

    private static bool[][] CreateGridWithLiveCentreAndNeighbours(int neighbourCount)
    {
        var cells = new bool[5][];
        for (int i = 0; i < 5; i++) cells[i] = new bool[5];
        cells[2][2] = true; // centre alive
        PlaceNeighbours(cells, 2, 2, neighbourCount);
        return cells;
    }

    private static bool[][] CreateGridWithDeadCentreAndNeighbours(int neighbourCount)
    {
        var cells = new bool[5][];
        for (int i = 0; i < 5; i++) cells[i] = new bool[5];
        cells[2][2] = false; // centre dead
        PlaceNeighbours(cells, 2, 2, neighbourCount);
        return cells;
    }

    /// <summary>Places up to 8 live cells in the Moore neighbourhood of (r,c).</summary>
    private static void PlaceNeighbours(bool[][] cells, int r, int c, int count)
    {
        int placed = 0;
        for (int dr = -1; dr <= 1 && placed < count; dr++)
            for (int dc = -1; dc <= 1 && placed < count; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                cells[r + dr][c + dc] = true;
                placed++;
            }
    }
}
