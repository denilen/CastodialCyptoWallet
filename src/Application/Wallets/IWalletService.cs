using CryptoWallet.Application.Common.Interfaces;
using CryptoWallet.Application.Wallets.Dtos;
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
}
