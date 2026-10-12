using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace UnitConversion.Api.Exceptions;

/// <summary>
/// Single place where exceptions become HTTP responses (RFC 9457 problem details).
/// Caller mistakes become 400 with their own message; anything else becomes a generic 500 so internal
/// details (messages, stack traces) are never exposed.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        if (exception is UnitConversionException conversionException)
        {
            logger.LogInformation("Rejected conversion request: {Message}", conversionException.Message);

            problem = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "The conversion request is invalid.",
                Detail = conversionException.Message,
                Extensions = { ["code"] = conversionException.ErrorCode }
            };
        }
        else
        {
            // Log the full exception server-side only.
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "The server could not process the request. Please try again later."
            };
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        var written = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });

        if (!written)
        {
            // The client's Accept header excluded problem+json; still return the body as plain JSON.
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        }

        return true;
    }
}
