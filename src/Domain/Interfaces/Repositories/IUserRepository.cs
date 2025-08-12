using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Users;

namespace CryptoWallet.Domain.Interfaces.Repositories;

/// <summary>
/// Repository for user data access operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The user if found; otherwise, null</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user with the specified email exists
    /// </summary>
    /// <param name="email">The email address to check</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>True if a user with the email exists; otherwise, false</returns>
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user with wallets for all available cryptocurrencies
    /// </summary>
    /// <param name="user">The user to create</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>The created user with associated wallets</returns>
    Task<User> CreateUserWithWalletsAsync(User user, CancellationToken cancellationToken = default);
}
