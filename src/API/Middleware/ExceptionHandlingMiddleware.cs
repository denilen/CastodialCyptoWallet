using System.Text.Json;
using CryptoWallet.API.Models;

namespace CryptoWallet.API.Middleware;

/// <summary>
/// Middleware for handling exceptions globally
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception
        _logger.LogError(exception, "An unhandled exception has occurred");

        // Set the response content type
        context.Response.ContentType = "application/json";

        // Create the response model
        var response = new ApiResponse<object>
        {
            Success = false,
            Error = GetErrorMessage(exception)
        };

        // Set the status code based on the exception type
        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        // Write the response
        var result = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(result);
    }

    private static string GetErrorMessage(Exception exception)
    {
        // Return a user-friendly error message based on the exception type
        return exception switch
        {
            UnauthorizedAccessException => "Access to the requested resource is not authorized.",
            KeyNotFoundException => "The requested resource was not found.",
            ArgumentException => "Invalid input provided: " + exception.Message,
            InvalidOperationException => "Operation could not be completed: " + exception.Message,
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}

/// <summary>
/// Extension method for adding the exception handling middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
