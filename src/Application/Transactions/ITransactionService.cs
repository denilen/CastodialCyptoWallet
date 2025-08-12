using Ardalis.Result;
using CryptoWallet.Application.Common.Interfaces;
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
    Task<Result<TransactionDto>> GetTransactionByIdAsync(
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
    Task<Result<PaginatedList<TransactionDto>>> GetWalletTransactionsAsync(
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
    Task<Result<PaginatedList<TransactionDto>>> GetUserTransactionsAsync(
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
    Task<Result<PaginatedList<TransactionDto>>> GetTransactionsByStatusAsync(
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
    Task<Result<PaginatedList<TransactionDto>>> GetTransactionsByTypeAsync(
        string type,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets transactions within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of transaction DTOs or an error</returns>
    Task<Result<PaginatedList<TransactionDto>>> GetTransactionsByDateRangeAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a transaction status
    /// </summary>
    /// <param name="transactionId">The transaction ID</param>
    /// <param name="status">The new status</param>
    /// <param name="notes">Optional notes about the status update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated transaction DTO or an error</returns>
    Task<Result<TransactionDto>> UpdateTransactionStatusAsync(
        Guid transactionId,
        string status,
        string? notes = null,
        CancellationToken cancellationToken = default);
}
