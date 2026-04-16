using GameOfLife.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameOfLife.Tests.Integration;

/// <summary>
/// Bootstraps the full ASP.NET Core pipeline for integration tests,
/// substituting the SQLite database with an in-memory EF Core provider.
/// </summary>
public sealed class GameOfLifeWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            services.RemoveAll<DbContextOptions<GameOfLifeDbContext>>();
            services.RemoveAll<GameOfLifeDbContext>();

            // Add in-memory database (unique per test run)
            var dbName = $"TestDb_{Guid.NewGuid()}";
            services.AddDbContext<GameOfLifeDbContext>(opts =>
                opts.UseInMemoryDatabase(dbName));

            // Ensure the schema is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GameOfLifeDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
