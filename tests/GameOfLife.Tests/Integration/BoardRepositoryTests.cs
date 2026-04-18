using FluentAssertions;
using GameOfLife.Core.Dtos;
using GameOfLife.Infrastructure.Persistence;
using GameOfLife.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GameOfLife.Tests.Integration;

/// <summary>
/// Tests <see cref="BoardRepository"/> against an in-memory EF Core database.
/// </summary>
public sealed class BoardRepositoryTests : IDisposable
{
    private readonly GameOfLifeDbContext _db;
    private readonly BoardRepository _repo;

    public BoardRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<GameOfLifeDbContext>()
            .UseInMemoryDatabase($"RepoTest_{Guid.NewGuid()}")
            .Options;

        _db = new GameOfLifeDbContext(options);
        _db.Database.EnsureCreated();
        _repo = new BoardRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    private static bool[][] SampleCells() => [[true, false], [false, true]];

    [Fact]
    public async Task SaveAsync_PersistsBoard_AndCanBeRetrieved()
    {
        var board = new BoardDto { Cells = SampleCells() };

        await _repo.SaveAsync(board);
        var fetched = await _repo.GetByIdAsync(board.Id);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(board.Id);
        fetched.Rows.Should().Be(2);
        fetched.Columns.Should().Be(2);
        fetched.Cells[0][0].Should().BeTrue();
        fetched.Cells[0][1].Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ModifiesCells_InDatabase()
    {
        var board = new BoardDto { Cells = SampleCells() };
        await _repo.SaveAsync(board);

        var updated = board.WithCells([[false, false], [false, false]]);
        await _repo.UpdateAsync(updated);

        var fetched = await _repo.GetByIdAsync(board.Id);
        fetched!.Cells[0][0].Should().BeFalse();
        fetched.Cells[1][1].Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_MultipleBoardsGetUniqueIds()
    {
        var a = new BoardDto { Cells = SampleCells() };
        var b = new BoardDto { Cells = SampleCells() };

        await _repo.SaveAsync(a);
        await _repo.SaveAsync(b);

        a.Id.Should().NotBe(b.Id);

        var fetchedA = await _repo.GetByIdAsync(a.Id);
        var fetchedB = await _repo.GetByIdAsync(b.Id);

        fetchedA.Should().NotBeNull();
        fetchedB.Should().NotBeNull();
    }
}
