using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Ardalis.Result;
using CryptoWallet.API.Models;
using CryptoWallet.API.Models.Wallets;
using CryptoWallet.Application.Wallets;
using CryptoWallet.Domain.Models.DTOs.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [FromBody] TransferRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Инициирован перевод средств с кошелька {SourceWallet} на кошелек {DestinationWallet} на сумму {Amount}",
            request.SourceWalletAddress, request.DestinationWalletAddress, request.Amount);

        // Получаем IP и User-Agent для аудита
        request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        request.UserAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _walletService.TransferFundsAsync(request, cancellationToken);

        return HandleResult(
            result,
            $"Успешно инициирован перевод {request.Amount} с кошелька {request.SourceWalletAddress} на {request.DestinationWalletAddress}");
    }

    /// <summary>
    /// Получить все кошельки пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список кошельков пользователя</returns>
    [HttpGet("user/{userId:guid}", Name = nameof(GetUserWallets))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<IReadOnlyList<WalletDto>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetUserWallets(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение всех кошельков пользователя с ID: {UserId}", userId);

        var result = await _walletService.GetUserWalletsByIdAsync(userId, cancellationToken);

        return HandleResult(
            result,
            $"Успешно получены кошельки пользователя с ID: {userId}");
    }

    /// <summary>
    /// Получить балансы по всем валютам пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Словарь с балансами по валютам</returns>
    [HttpGet("user/{userId:guid}/balance", Name = nameof(GetUserBalances))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<Dictionary<string, decimal>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetUserBalances(
        [FromRoute] Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение балансов по всем валютам пользователя с ID: {UserId}", userId);

        var result = await _walletService.GetUserBalancesAsync(userId, cancellationToken);

        return HandleResult(
            result,
            $"Успешно получены балансы пользователя с ID: {userId}");
    }

    /// <summary>
    /// Получить баланс пользователя по коду валюты
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="currency">Код валюты (например, "BTC", "ETH")</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Баланс пользователя в указанной валюте</returns>
    [HttpGet("user/{userId:guid}/balance/{currency}", Name = nameof(GetUserBalanceByCurrency))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<decimal>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetUserBalanceByCurrency(
        [FromRoute] Guid userId,
        [FromRoute] string currency,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение баланса пользователя {UserId} по валюте {Currency}", userId, currency);

        // First get the wallet to check if it exists and get the balance
        var walletResult = await _walletService.GetUserWalletByCurrencyAsync(userId, currency, cancellationToken);
        if (!walletResult.IsSuccess)
        {
            return HandleResult(Result<decimal>.Error(walletResult.Errors.FirstOrDefault() ?? "Wallet not found"));
        }

        return HandleResult(
            Result.Success(walletResult.Value.Balance),
            $"Успешно получен баланс пользователя {userId} в валюте {currency}");
    }

    /// <summary>
    /// Пополнить баланс пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="request">Данные для пополнения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о созданной транзакции</returns>
    [HttpPost("user/{userId:guid}/deposit", Name = nameof(DepositToUserWallet))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> DepositToUserWallet(
        [FromRoute] Guid userId,
        [FromBody] DepositToUserWalletRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Пополнение баланса пользователя {UserId} на сумму {Amount} {Currency}",
            userId, request.Amount, request.Currency);

        // Получаем IP и User-Agent для аудита
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _walletService.DepositToUserWalletAsync(
            userId: userId,
            currencyCode: request.Currency,
            amount: request.Amount,
            transactionHash: request.TransactionHash,
            cancellationToken: cancellationToken
        );

        return HandleResult(
            result,
            $"Успешно пополнен баланс пользователя {userId} на сумму {request.Amount} {request.Currency}");
    }

    /// <summary>
    /// Вывести средства с кошелька пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="request">Данные для вывода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о созданной транзакции</returns>
    [HttpPost("user/{userId:guid}/withdraw", Name = nameof(WithdrawFromUserWallet))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> WithdrawFromUserWallet(
        [FromRoute] Guid userId,
        [FromBody] WithdrawFromUserWalletRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Вывод средств пользователя {UserId} на сумму {Amount} {Currency} на адрес {DestinationAddress}",
            userId, request.Amount, request.Currency, request.DestinationAddress);

        // Получаем IP и User-Agent для аудита
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _walletService.WithdrawFromUserWalletAsync(
            userId: userId,
            currencyCode: request.Currency,
            amount: request.Amount,
            destinationAddress: request.DestinationAddress,
            cancellationToken: cancellationToken
        );

        return HandleResult(
            result,
            $"Успешно инициирован вывод {request.Amount} {request.Currency} пользователя {userId} на адрес {request.DestinationAddress}");
    }
}
