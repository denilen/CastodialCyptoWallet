using Ardalis.Result;
using CryptoWallet.Application.Common.Interfaces;
using CryptoWallet.Domain.Models.DTOs.Wallets;
using CryptoWallet.Domain.Users;

namespace CryptoWallet.Application.Wallets;

/// <summary>
/// Service for wallet management operations
/// </summary>
public interface IWalletService : IService
{
    /// <summary>
    /// Gets a wallet by its address
    /// </summary>
    /// <param name="address">Wallet address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet DTO or an error</returns>
    Task<Result<WalletDto>> GetWalletByAddressAsync(
        string address,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all wallets for a specific user
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wallet DTOs or an error</returns>
    Task<Result<IReadOnlyList<WalletDto>>> GetUserWalletsAsync(
        User user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's wallet for a specific cryptocurrency
    /// </summary>
    /// <param name="user">The user</param>
    /// <param name="currencyCode">Cryptocurrency code (e.g., "BTC", "ETH")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet DTO or an error</returns>
    Task<Result<WalletDto>> GetUserWalletByCurrencyAsync(
        User user,
        string currencyCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets wallet balance
    /// </summary>
    /// <param name="walletAddress">Wallet address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Balance information DTO or an error</returns>
    Task<Result<WalletBalanceDto>> GetWalletBalanceAsync(
        string walletAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deposits funds into a wallet
    /// </summary>
    /// <param name="request">Deposit request DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction DTO or an error</returns>
    Task<Result<TransactionDto>> DepositFundsAsync(
        DepositRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraws funds from a wallet
    /// </summary>
    /// <param name="request">Withdrawal request DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction DTO or an error</returns>
    Task<Result<TransactionDto>> WithdrawFundsAsync(
        WithdrawRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transfers funds between wallets
    /// </summary>
    /// <param name="request">Transfer request DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction DTO or an error</returns>
    Task<Result<TransactionDto>> TransferFundsAsync(
        TransferRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all wallets for a specific user by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of wallet DTOs or an error</returns>
    Task<Result<IReadOnlyList<WalletDto>>> GetUserWalletsByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's wallet for a specific cryptocurrency by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="currencyCode">Cryptocurrency code (e.g., "BTC", "ETH")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Wallet DTO or an error</returns>
    Task<Result<WalletDto>> GetUserWalletByCurrencyAsync(
        Guid userId,
        string currencyCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets balances for all wallets of a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of currency codes and balances, or an error</returns>
    Task<Result<Dictionary<string, decimal>>> GetUserBalancesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deposits funds to a user's wallet by currency
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="currencyCode">Currency code (e.g., "BTC", "ETH")</param>
    /// <param name="amount">Amount to deposit</param>
    /// <param name="transactionHash">Transaction hash (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction DTO or an error</returns>
    Task<Result<TransactionDto>> DepositToUserWalletAsync(
        Guid userId,
        string currencyCode,
        decimal amount,
        string? transactionHash = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Withdraws funds from a user's wallet by currency
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="currencyCode">Currency code (e.g., "BTC", "ETH")</param>
    /// <param name="amount">Amount to withdraw</param>
    /// <param name="destinationAddress">Destination wallet address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction DTO or an error</returns>
    Task<Result<TransactionDto>> WithdrawFromUserWalletAsync(
        Guid userId,
        string currencyCode,
        decimal amount,
        string destinationAddress,
        CancellationToken cancellationToken = default);
}
