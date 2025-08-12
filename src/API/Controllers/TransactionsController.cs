using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Ardalis.Result;
using AutoMapper;
using CryptoWallet.API.Models;
using CryptoWallet.API.Models.Transactions;
using CryptoWallet.Application.Common.Models;
using CryptoWallet.Application.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.API.Controllers;

/// <summary>
/// Контроллер для работы с транзакциями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<object>))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
[Authorize] // Требуем аутентификации для всех методов
public class TransactionsController : ApiControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;
    private readonly IMapper _mapper;

    public TransactionsController(
        ITransactionService transactionService,
        ILogger<TransactionsController> logger,
        IMapper mapper) : base(logger)
    {
        _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Получить транзакцию по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор транзакции</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о транзакции</returns>
    [HttpGet("{id:guid}", Name = nameof(GetTransactionById))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<Application.Transactions.Dtos.TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetTransactionById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение транзакции с ID: {TransactionId}", id);
        
        var result = await _transactionService.GetTransactionByIdAsync(id, cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно получена транзакция с ID: {id}");
    }

    /// <summary>
    /// Получить список транзакций с фильтрацией и пагинацией
    /// </summary>
    /// <param name="query">Параметры фильтрации и пагинации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Страница с отфильтрованными транзакциями</returns>
    [HttpGet(Name = nameof(GetTransactions))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginatedList<Application.Transactions.Dtos.TransactionDto>>))]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] GetTransactionsQueryDto query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение списка транзакций с параметрами: {Query}", query);
        
        // Apply filters using GetTransactionsByDateRangeAsync
        var startDate = query.StartDate ?? DateTimeOffset.UtcNow.AddDays(-30); // Default to last 30 days if not specified
        var endDate = query.EndDate ?? DateTimeOffset.UtcNow;
        
        var result = await _transactionService.GetTransactionsByDateRangeAsync(
            startDate: startDate,
            endDate: endDate,
            pageNumber: query.PageNumber,
            pageSize: query.PageSize,
            walletAddress: query.WalletAddress,
            status: query.Status,
            type: query.Type,
            minAmount: query.MinAmount,
            maxAmount: query.MaxAmount,
            cancellationToken: cancellationToken);
        
        return HandleResult(
            result,
            "Успешно получен список транзакций");
    }

    /// <summary>
    /// Обновить статус транзакции
    /// </summary>
    /// <param name="id">Идентификатор транзакции</param>
    /// <param name="request">Данные для обновления статуса</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Обновленная информация о транзакции</returns>
    [HttpPatch("{id:guid}/status", Name = nameof(UpdateTransactionStatus))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<Application.Transactions.Dtos.TransactionDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> UpdateTransactionStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateTransactionStatusDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Обновление статуса транзакции {TransactionId} на {Status}", id, request.Status);
        
        var result = await _transactionService.UpdateTransactionStatusAsync(
            id,
            request.Status,
            request.Notes,
            cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно обновлен статус транзакции {id} на {request.Status}");
    }

    /// <summary>
    /// Получить транзакции по адресу кошелька
    /// </summary>
    /// <param name="walletAddress">Адрес кошелька</param>
    /// <param name="pageNumber">Номер страницы (по умолчанию 1)</param>
    /// <param name="pageSize">Размер страницы (по умолчанию 20, максимум 100)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Страница с транзакциями кошелька</returns>
    [HttpGet("wallet/{walletAddress}", Name = nameof(GetWalletTransactions))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginatedList<Application.Transactions.Dtos.TransactionDto>>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetWalletTransactions(
        [FromRoute, Required] string walletAddress,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Получение транзакций для кошелька: {WalletAddress}", walletAddress);
        
        var result = await _transactionService.GetWalletTransactionsAsync(
            walletAddress,
            pageNumber,
            pageSize,
            cancellationToken);
        
        return HandleResult(
            result,
            $"Успешно получены транзакции для кошелька {walletAddress}");
    }
}
