
using Azure.Core;

namespace Products.Api.Constants;

public static class ApiErrorMessages
{
    public const string ValidationError = "Validation error";
    public const string ProductConflict = "Product conflict";
    public const string InternalServerError = "Internal server error";
    public const string UnexpectedError = "An unexpected error occurred.";
    public const string UnhandledExceptionError = "Unhandled exception while processing request.";
    public static string RequestFailedError(int StatusCode) => $"Request failed with status code {StatusCode}.";
}
