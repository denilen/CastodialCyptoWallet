using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Enums;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;

namespace CryptoWallet.Domain.Transactions;

/// <summary>
/// Defines the contract for transaction data access operations
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    /// Retrieves a paginated list of transactions for a specific wallet
    /// </summary>
    /// <param name="wallet">The wallet entity to get transactions for</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation, containing a paginated list of transactions</returns>
    Task<PaginatedList<Transaction>> GetWalletTransactionsAsync(
        Wallet wallet,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of transactions across all wallets for a specific user
    /// </summary>
    /// <param name="user">The user entity to get transactions for</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation, containing a paginated list of transactions</returns>
    Task<PaginatedList<Transaction>> GetUserTransactionsAsync(
        User user,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a transaction by its unique identifier with related data
    /// </summary>
    /// <param name="id">The unique identifier of the transaction</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation, containing the transaction with related data if found; otherwise, null</returns>
    Task<Transaction?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a transaction by its unique blockchain transaction hash
    /// </summary>
    /// <param name="transactionHash">The unique hash of the transaction on the blockchain</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation, containing the transaction if found; otherwise, null</returns>
    Task<Transaction?> GetByTransactionHashAsync(
        string transactionHash,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of transactions filtered by status
    /// </summary>
    /// <param name="statusEnum">The status to filter transactions by</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation, containing a paginated list of transactions with the specified status</returns>
    Task<PaginatedList<Transaction>> GetByStatusAsync(
        TransactionStatusEnum statusEnum,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
