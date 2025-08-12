using Ardalis.Result;
using CryptoWallet.Application.Common.Interfaces;
using CryptoWallet.Application.Users.Dtos;
using CryptoWallet.Domain.Users;

namespace CryptoWallet.Application.Users;

/// <summary>
/// Service for user management operations
/// </summary>
public interface IUserService : IService
{
    /// <summary>
    /// Registers a new user and creates default wallets
    /// </summary>
    /// <param name="request">User registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the created user or an error</returns>
    Task<Result<UserDto>> RegisterUserAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User DTO or null if not found</returns>
    Task<Result<UserDto>> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User DTO or null if not found</returns>
    Task<Result<UserDto>> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
}
