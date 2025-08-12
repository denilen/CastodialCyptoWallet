using System.Linq;
using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.API.Controllers;

/// <summary>
/// Базовый контроллер API, предоставляющий общую функциональность для всех контроллеров
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
    /// Обрабатывает результат операции и возвращает соответствующий IActionResult
    /// </summary>
    /// <typeparam name="T">Тип возвращаемых данных</typeparam>
    /// <param name="result">Результат операции</param>
    /// <param name="successMessage">Сообщение об успешном выполнении (опционально)</param>
    /// <returns>IActionResult с соответствующим статус-кодом</returns>
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
                .Select(v => new ValidationError(v.ErrorMessage, v.Identifier, v.ErrorCode, Ardalis.Result.ValidationSeverity.Error))
                .ToArray();
            nonGenericResult = Result.Invalid(validationErrors);
        }
        
        return HandleErrorResult(nonGenericResult);
    }

    /// <summary>
    /// Обрабатывает результат операции без возвращаемого значения
    /// </summary>
    /// <param name="result">Результат операции</param>
    /// <param name="successMessage">Сообщение об успешном выполнении (опционально)</param>
    /// <returns>IActionResult с соответствующим статус-кодом</returns>
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
    /// Обрабатывает ошибки результата операции
    /// </summary>
    private IActionResult HandleErrorResult(Result result)
    {
        if (result.Status == ResultStatus.NotFound)
        {
            _logger.LogWarning("Ресурс не найден: {Error}", result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            return NotFound(new { errors = result.Errors });
        }

        if (result.Status == ResultStatus.Invalid)
        {
            _logger.LogWarning("Некорректный запрос: {Error}", string.Join("; ", result.ValidationErrors));
            return BadRequest(new { errors = result.ValidationErrors });
        }

        if (result.Status == ResultStatus.Unauthorized)
        {
            _logger.LogWarning("Ошибка авторизации: {Error}", result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
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
