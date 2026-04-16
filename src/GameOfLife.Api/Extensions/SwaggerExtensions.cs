using System.Reflection;
using Microsoft.OpenApi.Models;

namespace GameOfLife.Api.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen
        (
            options =>
            {
                options.SwaggerDoc
                (
                    "v1", 
                    new OpenApiInfo
                    {
                        Title = "Game of Life API",
                        Version = "v1",
                        Description = "A RESTful API for Conway's Game of Life. " +
                                      "Upload board states and compute next, N-step, and final stable generations.",
                        Contact = new OpenApiContact { Name = "Game of Life API" }
                    }
                );

                // Include XML documentation comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            }
        );

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI
        (
            options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Game of Life API v1");
                options.RoutePrefix = string.Empty; // Serve Swagger UI at root
            }
        );
        return app;
    }
}
