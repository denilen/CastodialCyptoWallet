using System.Data;
using CryptoWallet.Domain.Interfaces.Repositories;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;
using CryptoWallet.Infrastructure.Extensions;
using CryptoWallet.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace CryptoWallet.Infrastructure.Persistence.Repositories;

/// <summary>
/// Реализация репозитория для работы с пользователями
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));

        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));

        return await DbSet
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<User> CreateUserWithWalletsAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // Start transaction
        await using var transaction = await Context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            // Check if user with this email already exists
            if (await ExistsWithEmailAsync(user.Email, cancellationToken))
            {
                throw new InvalidOperationException($"User with email '{user.Email}' already exists.");
            }

            // Add user
            await DbSet.AddAsync(user, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);

            // Get all active cryptocurrencies
            var cryptocurrencies = await Context.Cryptocurrencies
                .Where(c => c.IsActive)
                .ToListAsync(cancellationToken);

            if (!cryptocurrencies.Any())
            {
                throw new InvalidOperationException("No active cryptocurrencies found to create wallets.");
            }

            // Create wallets for each cryptocurrency
            var wallets = new List<Wallet>();
            foreach (var crypto in cryptocurrencies)
            {
                // Generate unique wallet address
                var walletAddress = GenerateWalletAddress(crypto.Code, user.Id);
                var wallet = new Wallet(user, crypto, walletAddress);
                
                // Initialize collections to avoid null reference exceptions
                if (crypto.Wallets == null)
                {
                    crypto.GetType().GetProperty("Wallets")?.SetValue(crypto, new List<Wallet>());
                }
                
                wallets.Add(wallet);
            }

            // Add wallets
            await Context.Wallets.AddRangeAsync(wallets, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);

            // Commit transaction
            await transaction.CommitAsync(cancellationToken);

            // Return user with loaded wallets
            return await DbSet
                .Include(u => u.Wallets)
                .ThenInclude(w => w.Cryptocurrency)
                .FirstAsync(u => u.Id == user.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the exception if needed
            await transaction.RollbackAsync(cancellationToken);
            throw new InvalidOperationException("Failed to create user with wallets. See inner exception for details.", ex);
        }
    }

    private static string GenerateWalletAddress(string cryptoCode, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(cryptoCode))
            throw new ArgumentException("Cryptocurrency code cannot be null or whitespace.", nameof(cryptoCode));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        string address;
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = new Random().Next(1000, 9999);
        
        // Generate a basic address based on the crypto code and user ID
        address = $"{cryptoCode.ToLower()}_{userId.ToString("N")[..8]}_{timestamp}_{random}";
        
        // Ensure the generated address meets our validation requirements
        if (!address.IsValidWalletAddress(cryptoCode))
        {
            // If the basic pattern doesn't match, fall back to a more generic but valid format
            address = $"wallet_{cryptoCode.ToLower()}_{userId:N}_{timestamp}";
            
            // If still not valid, throw an exception
            if (!address.IsValidWalletAddress())
            {
                throw new InvalidOperationException("Failed to generate a valid wallet address.");
            }
        }
        
        return address;
    }
}
