using GameOfLife.Infrastructure.Persistence.Entities;
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
