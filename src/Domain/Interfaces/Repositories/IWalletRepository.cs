using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Currencies;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;

namespace CryptoWallet.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for wallet operations
/// </summary>
public interface IWalletRepository : IRepository<Wallet>
{
    /// <summary>
    /// Gets a user's wallet by cryptocurrency
    /// </summary>
    /// <param name="user">The user entity</param>
    /// <param name="cryptocurrency">The cryptocurrency entity</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The wallet if found; otherwise, null</returns>
    Task<Wallet?> GetUserWalletByCurrencyAsync(
        User user,
        Cryptocurrency cryptocurrency,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all wallets for a user with cryptocurrency details
    /// </summary>
    /// <param name="user">The user entity</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A read-only list of the user's wallets</returns>
    Task<IReadOnlyList<Wallet>> GetUserWalletsWithDetailsAsync(
        User user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a wallet by its address with user and cryptocurrency details
    /// </summary>
    /// <param name="address">The wallet address</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The wallet with details if found; otherwise, null</returns>
    Task<Wallet?> GetByAddressWithDetailsAsync(
        string address,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a wallet exists with the specified address
    /// </summary>
    /// <param name="address">The wallet address to check</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>True if a wallet with the address exists; otherwise, false</returns>
    Task<bool> ExistsWithAddressAsync(
        string address,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a wallet by its address
    /// </summary>
    /// <param name="address">The wallet address</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The wallet if found; otherwise, null</returns>
    Task<Wallet?> GetByAddressAsync(
        string address,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a wallet by its ID with user and cryptocurrency details
    /// </summary>
    /// <param name="id">The wallet ID</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The wallet with details if found; otherwise, null</returns>
    Task<Wallet?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all wallets for a user with details by user ID
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A read-only list of the user's wallets with details</returns>
    Task<IReadOnlyList<Wallet>> GetUserWalletsByIdWithDetailsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's wallet by currency with details
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="currency">The cryptocurrency code</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The wallet if found; otherwise, null</returns>
    Task<Wallet?> GetUserWalletByCurrencyWithDetailsAsync(
        Guid userId,
        string currency,
        CancellationToken cancellationToken = default);
}
