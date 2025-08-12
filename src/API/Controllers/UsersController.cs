using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Ardalis.Result;
using AutoMapper;
using CryptoWallet.API.Models;
using CryptoWallet.API.Models.Users;
using CryptoWallet.Application.Common.Models;
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

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger) : base(logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Зарегистрировать нового пользователя
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о зарегистрированном пользователе</returns>
    [HttpPost("register", Name = nameof(Register))]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<UserDto>))]
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
            LastName = request.LastName,
            PhoneNumber = null, // Not provided in the request DTO
            CountryCode = null, // Not provided in the request DTO
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        };

        var result = await _userService.RegisterUserAsync(registerRequest, cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Value.Id },
            ApiResponse<UserDto>.SuccessResponse(result.Value));
    }

    /// <summary>
    /// Получить информацию о пользователе по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о пользователе</returns>
    [HttpGet("{id:guid}", Name = nameof(GetUserById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UserDto>))]
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

        return Ok(ApiResponse<UserDto>.SuccessResponse(result.Value));
    }

    // Wallet and transaction related methods have been moved to WalletsController
}
