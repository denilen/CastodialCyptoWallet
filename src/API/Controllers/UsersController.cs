using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Ardalis.Result;
using AutoMapper;
using CryptoWallet.API.Models;
using CryptoWallet.API.Models.Users;
using CryptoWallet.Application.Users;
using CryptoWallet.Application.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.API.Controllers;

/// <summary>
/// Контроллер для работы с пользователями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[AllowAnonymous] // Разрешаем доступ без аутентификации для регистрации и входа
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly IMapper _mapper;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger,
        IMapper mapper) : base(logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Зарегистрировать нового пользователя
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о зарегистрированном пользователе</returns>
    [HttpPost("register", Name = nameof(Register))]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<UserResponseDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Попытка регистрации пользователя с email: {Email}", request.Email);
        
        var registerRequest = new RegisterUserRequest
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        
        var result = await _userService.RegisterUserAsync(registerRequest, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }
        
        var userDto = _mapper.Map<UserResponseDto>(result.Value);
        
        return CreatedAtAction(
            nameof(GetUserById),
            new { id = userDto.Id },
            ApiResponse<UserResponseDto>.SuccessResponse(userDto));
    }

    /// <summary>
    /// Получить информацию о пользователе по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о пользователе</returns>
    [HttpGet("{id:guid}", Name = nameof(GetUserById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UserResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    [Authorize] // Требуем аутентификации
    public async Task<IActionResult> GetUserById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение информации о пользователе с ID: {UserId}", id);
        
        var result = await _userService.GetUserByIdAsync(id, cancellationToken);
        
        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }
        
        var userDto = _mapper.Map<UserResponseDto>(result.Value);
        
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(userDto));
    }

    /// <summary>
    /// Получить баланс пользователя в указанной валюте
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="currency">Код валюты (например, "BTC", "ETH")</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Баланс пользователя в указанной валюте</returns>
    [HttpGet("{id:guid}/balance", Name = nameof(GetUserBalance))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<decimal>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    [Authorize] // Требуем аутентификации
    public async Task<IActionResult> GetUserBalance(
        [FromRoute] Guid id,
        [FromQuery, Required] string currency,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение баланса пользователя {UserId} в валюте {Currency}", id, currency);
        
        var result = await _userService.GetUserBalanceAsync(id, currency, cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно получен баланс пользователя {id} в валюте {currency}");
    }

    /// <summary>
    /// Пополнить баланс пользователя
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="currency">Код валюты (например, "BTC", "ETH")</param>
    /// <param name="amount">Сумма для пополнения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    [HttpPost("{id:guid}/deposit", Name = nameof(DepositFunds))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    [Authorize] // Требуем аутентификации
    public async Task<IActionResult> DepositFunds(
        [FromRoute] Guid id,
        [FromQuery, Required] string currency,
        [FromQuery, Required, Range(0.0001, double.MaxValue)] decimal amount,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Пополнение баланса пользователя {UserId} на {Amount} {Currency}", 
            id, amount, currency);
        
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var result = await _userService.DepositFundsAsync(
            id, 
            currency, 
            amount,
            ipAddress,
            userAgent,
            cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно пополнен баланс пользователя {id} на сумму {amount} {currency}");
    }

    /// <summary>
    /// Вывести средства с баланса пользователя
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="currency">Код валюты (например, "BTC", "ETH")</param>
    /// <param name="amount">Сумма для вывода</param>
    /// <param name="destinationAddress">Адрес назначения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    [HttpPost("{id:guid}/withdraw", Name = nameof(WithdrawFunds))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    [Authorize] // Требуем аутентификации
    public async Task<IActionResult> WithdrawFunds(
        [FromRoute] Guid id,
        [FromQuery, Required] string currency,
        [FromQuery, Required, Range(0.0001, double.MaxValue)] decimal amount,
        [FromQuery, Required] string destinationAddress,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Вывод {Amount} {Currency} с баланса пользователя {UserId} на адрес {Destination}", 
            amount, currency, id, destinationAddress);
        
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var result = await _userService.WithdrawFundsAsync(
            id,
            currency,
            amount,
            destinationAddress,
            ipAddress,
            userAgent,
            cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно инициирован вывод {amount} {currency} с баланса пользователя {id}");
    }
}
