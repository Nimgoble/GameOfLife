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
        // Detect provider based on the connection string. Default is SQLite for local dev.
        var cs = connectionString ?? "Data Source=gameoflife.db";
        services.AddDbContext<GameOfLifeDbContext>(opts =>
        {
            var lower = cs.ToLowerInvariant();
            var useSqlServer = lower.Contains("server=") || lower.Contains("data source=") && lower.Contains(".database.windows.net");

            if (useSqlServer)
            {
                // Use SQL Server provider. For Azure-managed identity scenarios include
                // "Authentication=Active Directory Default" in the connection string; the
                // underlying SqlClient will use the platform identity to authenticate.
                // Enable resilient retries for transient SQL errors. Limit retries to 3.
                opts.UseSqlServer(cs, sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 3));
            }
            else
            {
                opts.UseSqlite(cs);
            }
        });

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
