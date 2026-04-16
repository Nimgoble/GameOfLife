using System.Security.Cryptography;
using System.Text;
using GameOfLife.Core.Interfaces;

namespace GameOfLife.Api.Services;

/// <summary>
/// Implements the standard Conway's Game of Life rules:
/// <list type="bullet">
///   <item>A live cell with 2 or 3 live neighbours survives.</item>
///   <item>A dead cell with exactly 3 live neighbours becomes alive.</item>
///   <item>All other cells die or remain dead.</item>
/// </list>
/// </summary>
public sealed class BoardEvolver : IBoardEvolver
{
    public bool[][] NextGeneration(bool[][] cells)
    {
        if (cells.Length == 0) return [];

        int rows = cells.Length;
        int cols = cells[0].Length;
        var next = CreateGrid(rows, cols);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int liveNeighbours = CountLiveNeighbours(cells, r, c, rows, cols);
                next[r][c] = cells[r][c]
                    ? liveNeighbours is 2 or 3      // survival
                    : liveNeighbours is 3;          // reproduction
            }
        }

        return next;
    }

    public string ComputeHash(bool[][] cells)
    {
        // Encode the grid as a compact bit string, then SHA-256 hash it.
        var sb = new StringBuilder(cells.Length * (cells.Length > 0 ? cells[0].Length : 0));
        foreach (var row in cells)
        {
            foreach (var cell in row)
            {
                sb.Append(cell ? '1' : '0');
            }
        }            

        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private static int CountLiveNeighbours(bool[][] cells, int row, int col, int rows, int cols)
    {
        int count = 0;
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = row + dr;
                int nc = col + dc;
                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && cells[nr][nc])
                    count++;
            }
        }
        return count;
    }

    private static bool[][] CreateGrid(int rows, int cols)
    {
        var grid = new bool[rows][];
        for (int i = 0; i < rows; i++)
            grid[i] = new bool[cols];
        return grid;
    }
}
