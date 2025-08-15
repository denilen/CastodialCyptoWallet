using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace CryptoWallet.API.Controllers;

/// <summary>
/// Base API controller that provides common functionality for all controllers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private readonly ILogger _logger;

    protected ApiControllerBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the operation result and returns the appropriate IActionResult
    /// </summary>
    /// <typeparam name="T">Type of the returned data</typeparam>
    /// <param name="result">Operation result</param>
    /// <param name="successMessage">Success message (optional)</param>
    /// <returns>IActionResult with the appropriate status code</returns>
    protected IActionResult HandleResult<T>(Result<T> result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                _logger.LogInformation(successMessage);
            }

            return Ok(result.Value);
        }

        // Convert the generic Result<T> to a non-generic Result for error handling
        var nonGenericResult = Result.Error(string.Join("; ", result.Errors));
        if (result.ValidationErrors != null && result.ValidationErrors.Any())
        {
            var validationErrors = result.ValidationErrors
                .Select(v => new ValidationError(v.ErrorMessage, v.Identifier, v.ErrorCode,
                    Ardalis.Result.ValidationSeverity.Error))
                .ToArray();
            nonGenericResult = Result.Invalid(validationErrors);
        }

        return HandleErrorResult(nonGenericResult);
    }

    /// <summary>
    /// Processes an operation result without a return value
    /// </summary>
    /// <param name="result">Operation result</param>
    /// <param name="successMessage">Success message (optional)</param>
    /// <returns>IActionResult with the appropriate status code</returns>
    protected IActionResult HandleResult(Result result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                _logger.LogInformation(successMessage);
            }

            return NoContent();
        }

        return HandleErrorResult(result);
    }

    /// <summary>
    /// Handles operation result errors
    /// </summary>
    private IActionResult HandleErrorResult(Result result)
    {
        if (result.Status == ResultStatus.NotFound)
        {
            _logger.LogWarning("Resource not found: {Error}", result.Errors.FirstOrDefault() ?? "Unknown error");
            return NotFound(new { errors = result.Errors });
        }

        if (result.Status == ResultStatus.Invalid)
        {
            _logger.LogWarning("Invalid request: {Error}", string.Join("; ", result.ValidationErrors));
            return BadRequest(new { errors = result.ValidationErrors });
        }

        if (result.Status == ResultStatus.Unauthorized)
        {
            _logger.LogWarning("Authorization error: {Error}", result.Errors.FirstOrDefault() ?? "Unknown error");
            return Unauthorized(new { errors = result.Errors });
        }

        if (result.Status == ResultStatus.Forbidden)
        {
            _logger.LogWarning("Доступ запрещен: {Error}", result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            return StatusCode(403, new { errors = result.Errors });
        }

        _logger.LogError("Внутренняя ошибка сервера: {Error}", string.Join("; ", result.Errors));
        return StatusCode(500, new { errors = result.Errors });
    }
}
