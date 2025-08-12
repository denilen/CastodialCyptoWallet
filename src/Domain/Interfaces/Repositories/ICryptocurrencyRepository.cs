using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Currencies;

namespace CryptoWallet.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for cryptocurrency operations
/// </summary>
public interface ICryptocurrencyRepository : IRepository<Cryptocurrency>
{
    /// <summary>
    /// Gets a cryptocurrency by its code
    /// </summary>
    /// <param name="code">The cryptocurrency code (e.g., "BTC", "ETH")</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The cryptocurrency if found; otherwise, null</returns>
    Task<Cryptocurrency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active cryptocurrencies
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A read-only list of active cryptocurrencies</returns>
    Task<IReadOnlyList<Cryptocurrency>> GetActiveCurrenciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a cryptocurrency with the given code exists
    /// </summary>
    /// <param name="code">The cryptocurrency code to check</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>True if a cryptocurrency with the code exists; otherwise, false</returns>
    Task<bool> ExistsWithCodeAsync(string code, CancellationToken cancellationToken = default);
}
