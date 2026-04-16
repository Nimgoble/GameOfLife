using GameOfLife.Api.Models;
using GameOfLife.Api.Services;
using GameOfLife.Core.Interfaces;

namespace GameOfLife.Api.Extensions;

public static class ApplicationServiceExtensions
{
    /// <summary>Registers all application-layer services.</summary>
    public static IServiceCollection AddApplicationServices
    (
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddOptions<GameOfLifeOptions>()
                .Bind(configuration.GetSection(GameOfLifeOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddSingleton<IBoardEvolver, BoardEvolver>();
        services.AddScoped<IGameOfLifeService, GameOfLifeService>();
        services.AddScoped<BoardValidator>();

        return services;
    }
}
