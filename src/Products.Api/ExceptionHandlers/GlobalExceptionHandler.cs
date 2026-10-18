
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Products.Api.Constants;
using Products.Application.Exceptions;

namespace Products.Api.ExceptionHandlers;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ProductValidationException or ArgumentException =>
                (StatusCodes.Status400BadRequest,
                    ApiErrorMessages.ValidationError),

            ProductConflictException =>
                (StatusCodes.Status409Conflict,
                    ApiErrorMessages.ProductConflict),

            _ =>
                (StatusCodes.Status500InternalServerError,
                    ApiErrorMessages.InternalServerError)
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                ApiErrorMessages.UnhandledExceptionError);
        }
        else
        {
            logger.LogWarning(
                exception,
                ApiErrorMessages.RequestFailedError(statusCode));
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode >= StatusCodes.Status500InternalServerError
                ? ApiErrorMessages.UnexpectedError
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
