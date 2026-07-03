using Confluent.Kafka;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IdentityService.Common;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, code, detail) = MapException(exception);

        logger.LogError(exception, "Unhandled exception: {Code}", code);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.io/{statusCode}",
            title = "An error occurred while processing your request.",
            status = statusCode,
            detail,
            code,
            traceId = httpContext.TraceIdentifier
        }, ct);

        return true;
    }

    private static (int StatusCode, string Code, string Detail) MapException(Exception exception) => exception switch
    {
        DbUpdateException { InnerException: PostgresException { SqlState: "23505" } } =>
            (
                StatusCodes.Status409Conflict, 
                "Identity.DuplicateKey", 
                "A record with conflicting unique data already exists."
            ),

        DbUpdateException =>
            (
                StatusCodes.Status500InternalServerError, 
                "Identity.DatabaseError", 
                "A database error occurred."
            ),

        ProduceException<string, string> =>
            (
                StatusCodes.Status500InternalServerError, 
                "Identity.KafkaUnavailable", 
                "Failed to publish event to message broker."
            ),

        OperationCanceledException =>
            (
                StatusCodes.Status499ClientClosedRequest, 
                "Identity.RequestCancelled", 
                "The request was cancelled."
            ),

        _ =>
            (
                StatusCodes.Status500InternalServerError, 
                "Identity.Unexpected", 
                "An unexpected error occurred."
            )
    };
}