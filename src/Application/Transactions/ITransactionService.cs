using CryptoWallet.Application.Common.Interfaces;
using CryptoWallet.Application.Common.Models;
using CryptoWallet.Application.Transactions.Dtos;
using CryptoWallet.Domain.Users;

namespace CryptoWallet.Application.Transactions;

/// <summary>
/// Service for transaction management operations
/// </summary>
public interface ITransactionService : IService
{
    /// <summary>
    /// Gets a transaction by its ID
    /// </summary>
    /// <param name="transactionId">The transaction ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction DTO or an error</returns>
    Task<Ardalis.Result.Result<TransactionDto>> GetTransactionByIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transactions for a specific wallet
    /// </summary>
    /// <param name="walletAddress">The wallet address</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of transaction DTOs or an error</returns>
    Task<Ardalis.Result.Result<PaginatedList<TransactionDto>>> GetWalletTransactionsAsync(
        string walletAddress,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transactions for a specific user
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of transaction DTOs or an error</returns>
    Task<Ardalis.Result.Result<PaginatedList<TransactionDto>>> GetUserTransactionsAsync(
        User user,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by status
    /// </summary>
    /// <param name="status">Transaction status</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of transaction DTOs or an error</returns>
    Task<Ardalis.Result.Result<PaginatedList<TransactionDto>>> GetTransactionsByStatusAsync(
        string status,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions by type
    /// </summary>
    /// <param name="type">Transaction type</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of transaction DTOs or an error</returns>
    Task<Ardalis.Result.Result<PaginatedList<TransactionDto>>> GetTransactionsByTypeAsync(
        string type,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions within a date range with optional filtering
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="walletAddress">Optional wallet address to filter by</param>
    /// <param name="status">Optional status to filter by</param>
    /// <param name="type">Optional transaction type to filter by</param>
    /// <param name="minAmount">Optional minimum amount to filter by</param>
    /// <param name="maxAmount">Optional maximum amount to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of transaction DTOs or an error</returns>
    Task<Ardalis.Result.Result<PaginatedList<TransactionDto>>> GetTransactionsByDateRangeAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        int pageNumber = 1,
        int pageSize = 20,
        string? walletAddress = null,
        string? status = null,
        string? type = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a transaction status
    /// </summary>
    /// <param name="transactionId">The transaction ID</param>
    /// <param name="status">The new status</param>
    /// <param name="notes">Optional notes about the status update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated transaction DTO or an error</returns>
    Task<Ardalis.Result.Result<TransactionDto>> UpdateTransactionStatusAsync(
        Guid transactionId,
        string status,
        string? notes = null,
        CancellationToken cancellationToken = default);
}
