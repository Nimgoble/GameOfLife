using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Infrastructure.Persistence;

/// <summary>
/// EF Core database context. Uses SQLite by default; can be swapped via
/// connection string for SQL Server, PostgreSQL, etc.
/// </summary>
public sealed class GameOfLifeDbContext(DbContextOptions<GameOfLifeDbContext> options)
    : DbContext(options)
{
    public DbSet<BoardRecord> Boards => Set<BoardRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BoardRecord>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Id).ValueGeneratedNever();
            entity.Property(b => b.CellsJson)
                  .HasColumnName("Cells")
                  .IsRequired();
            entity.Property(b => b.CreatedAt).IsRequired();
        });
    }
}

/// <summary>Flat persistence record for a board.</summary>
public sealed class BoardRecord
{
    public Guid Id { get; set; }

    /// <summary>JSON-serialised 2-D boolean array.</summary>
    public string CellsJson { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }
}
