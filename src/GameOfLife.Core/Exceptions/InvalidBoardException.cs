namespace GameOfLife.Core.Exceptions;

/// <summary>Thrown when board input fails validation.</summary>
public sealed class InvalidBoardException(string message) : Exception(message);
