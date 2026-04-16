using System.Net;
using System.Text.Json;
using GameOfLife.Api.Models;
using GameOfLife.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions from the pipeline and converts them to
/// well-structured JSON error responses.  This keeps controllers free of
/// try/catch boilerplate and ensures a consistent error contract for clients.
/// </summary>
public sealed class ExceptionHandlingMiddleware
(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteErrorAsync(context, ex);
        }
    }

    private static Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, message) = MapException(exception);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = code,
            Detail = message,
            Instance = context.Request.Path
        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
    }

    private static (HttpStatusCode status, string code, string message) MapException(Exception exception) => 
        exception switch
        {
            BoardNotFoundException ex =>
                (HttpStatusCode.NotFound, "BOARD_NOT_FOUND", ex.Message),

            InvalidBoardException ex =>
                (HttpStatusCode.BadRequest, "INVALID_BOARD", ex.Message),

            BoardDidNotStabiliseException ex =>
                (HttpStatusCode.UnprocessableEntity, "BOARD_DID_NOT_STABILISE", ex.Message),

            OperationCanceledException =>
                (HttpStatusCode.ServiceUnavailable, "REQUEST_CANCELLED", "The request was cancelled."),

            _ =>
                (HttpStatusCode.InternalServerError, "INTERNAL_ERROR",
                    "An unexpected error occurred. Please try again later.")
        };
}
