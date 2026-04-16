using GameOfLife.Core.Interfaces;
using GameOfLife.Infrastructure.Persistence;
using GameOfLife.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameOfLife.Infrastructure;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers all infrastructure services (EF Core + repository).
    /// <paramref name="connectionString"/> defaults to a local SQLite file.
    /// </summary>
    public static IServiceCollection AddInfrastructure
    (
        this IServiceCollection services,
        string connectionString = "Data Source=gameoflife.db"
    )
    {
        services.AddDbContext<GameOfLifeDbContext>
        (
            opts =>
            opts.UseSqlite(connectionString)
        );

        services.AddScoped<IBoardRepository, BoardRepository>();
        return services;
    }

    /// <summary>Applies any pending EF Core migrations on startup.</summary>
    public static void ApplyMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameOfLifeDbContext>();
        db.Database.EnsureCreated();
    }
}
