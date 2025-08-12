using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Ardalis.Result;
using CryptoWallet.API.Models;
using CryptoWallet.API.Models.Wallets;
using CryptoWallet.Application.Wallets;
using CryptoWallet.Application.Wallets.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.API.Controllers;

/// <summary>
/// Контроллер для работы с криптовалютными кошельками
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize] // Требуем аутентификации для всех методов
public class WalletsController : ApiControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletsController> _logger;

    public WalletsController(
        IWalletService walletService,
        ILogger<WalletsController> logger) : base(logger)
    {
        _walletService = walletService ?? throw new ArgumentNullException(nameof(walletService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Получить информацию о кошельке по адресу
    /// </summary>
    /// <param name="address">Адрес кошелька</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о кошельке</returns>
    [HttpGet("{address}", Name = nameof(GetWalletByAddress))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<WalletDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetWalletByAddress(
        [FromRoute, Required] string address,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение информации о кошельке с адресом: {Address}", address);
        
        var result = await _walletService.GetWalletByAddressAsync(address, cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно получена информация о кошельке {address}");
    }

    /// <summary>
    /// Получить баланс кошелька
    /// </summary>
    /// <param name="address">Адрес кошелька</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Баланс кошелька</returns>
    [HttpGet("{address}/balance", Name = nameof(GetWalletBalance))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<WalletBalanceDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetWalletBalance(
        [FromRoute, Required] string address,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение баланса кошелька с адресом: {Address}", address);
        
        var result = await _walletService.GetWalletBalanceAsync(address, cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно получен баланс кошелька {address}");
    }

    /// <summary>
    /// Пополнить баланс кошелька
    /// </summary>
    /// <param name="address">Адрес кошелька</param>
    /// <param name="request">Данные для пополнения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о созданной транзакции</returns>
    [HttpPost("{address}/deposit", Name = nameof(DepositFunds))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> DepositFunds(
        [FromRoute, Required] string address,
        [FromBody] DepositRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Пополнение кошелька {Address} на сумму {Amount}", address, request.Amount);
        
        // Получаем IP-адрес и User-Agent из контекста запроса
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var depositRequest = new DepositRequest
        {
            WalletAddress = address,
            Amount = request.Amount,
            TransactionHash = request.TransactionHash,
            Notes = request.Notes,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
        
        var result = await _walletService.DepositFundsAsync(depositRequest, cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно пополнен баланс кошелька {address} на сумму {request.Amount}");
    }

    /// <summary>
    /// Вывести средства с кошелька
    /// </summary>
    /// <param name="address">Адрес кошелька</param>
    /// <param name="request">Данные для вывода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о созданной транзакции</returns>
    [HttpPost("{address}/withdraw", Name = nameof(WithdrawFunds))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> WithdrawFunds(
        [FromRoute, Required] string address,
        [FromBody] WithdrawRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Вывод средств с кошелька {Address} на сумму {Amount}", address, request.Amount);
        
        // Получаем IP-адрес и User-Agent из контекста запроса
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var withdrawRequest = new WithdrawRequest
        {
            SourceWalletAddress = address,
            DestinationAddress = request.DestinationAddress,
            Amount = request.Amount,
            Fee = request.Fee,
            Notes = request.Notes,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
        
        var result = await _walletService.WithdrawFundsAsync(withdrawRequest, cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно инициирован вывод {request.Amount} с кошелька {address}");
    }

    /// <summary>
    /// Перевести средства между кошельками
    /// </summary>
    /// <param name="request">Данные для перевода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о созданной транзакции</returns>
    [HttpPost("transfer", Name = nameof(TransferFunds))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> TransferFunds(
        [FromBody] TransferRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Перевод {Amount} с кошелька {Source} на кошелек {Destination}", 
            request.Amount, request.SourceWalletAddress, request.DestinationWalletAddress);
        
        // Получаем IP-адрес и User-Agent из контекста запроса
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        
        var transferRequest = new TransferRequest
        {
            SourceWalletAddress = request.SourceWalletAddress,
            DestinationWalletAddress = request.DestinationWalletAddress,
            Amount = request.Amount,
            Fee = request.Fee,
            Notes = request.Notes,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
        
        var result = await _walletService.TransferFundsAsync(transferRequest, cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно переведено {request.Amount} с кошелька {request.SourceWalletAddress} на кошелек {request.DestinationWalletAddress}");
    }
}
