using FluentAssertions;
using GameOfLife.Api.Models;
using GameOfLife.Api.Services;
using GameOfLife.Core.Exceptions;
using Microsoft.Extensions.Options;
using Xunit;

namespace GameOfLife.Tests.Unit;

public sealed class BoardValidatorTests
{
    private static BoardValidator CreateValidator(int maxDim = 100) =>
        new(Options.Create(new GameOfLifeOptions { MaxBoardDimension = maxDim }));

    [Fact]
    public void Validate_ValidSquareGrid_DoesNotThrow()
    {
        var validator = CreateValidator();
        bool[][] cells = [[true, false], [false, true]];
        validator.Invoking(v => v.Validate(cells)).Should().NotThrow();
    }

    [Fact]
    public void Validate_ValidRectangularGrid_DoesNotThrow()
    {
        var validator = CreateValidator();
        bool[][] cells = [[true, false, true], [false, true, false]];
        validator.Invoking(v => v.Validate(cells)).Should().NotThrow();
    }

    [Fact]
    public void Validate_NullCells_ThrowsInvalidBoardException()
    {
        var validator = CreateValidator();
        validator.Invoking(v => v.Validate(null!))
                 .Should().Throw<InvalidBoardException>();
    }

    [Fact]
    public void Validate_EmptyGrid_ThrowsInvalidBoardException()
    {
        var validator = CreateValidator();
        validator.Invoking(v => v.Validate([]))
                 .Should().Throw<InvalidBoardException>()
                 .WithMessage("*at least one row*");
    }

    [Fact]
    public void Validate_RowWithZeroCells_ThrowsInvalidBoardException()
    {
        var validator = CreateValidator();
        bool[][] cells = [[]];
        validator.Invoking(v => v.Validate(cells))
                 .Should().Throw<InvalidBoardException>()
                 .WithMessage("*at least one cell*");
    }

    [Fact]
    public void Validate_JaggedGrid_ThrowsInvalidBoardException()
    {
        var validator = CreateValidator();
        bool[][] cells = [[true, false], [true]]; // row 1 has 1 cell, row 0 has 2
        validator.Invoking(v => v.Validate(cells))
                 .Should().Throw<InvalidBoardException>()
                 .WithMessage("*same length*");
    }

    [Fact]
    public void Validate_TooManyRows_ThrowsInvalidBoardException()
    {
        var validator = CreateValidator(maxDim: 3);
        var cells = Enumerable.Range(0, 4).Select(_ => new bool[1]).ToArray();
        validator.Invoking(v => v.Validate(cells))
                 .Should().Throw<InvalidBoardException>()
                 .WithMessage("*row count*");
    }

    [Fact]
    public void Validate_TooManyColumns_ThrowsInvalidBoardException()
    {
        var validator = CreateValidator(maxDim: 3);
        bool[][] cells = [new bool[4]];
        validator.Invoking(v => v.Validate(cells))
                 .Should().Throw<InvalidBoardException>()
                 .WithMessage("*column count*");
    }
}
