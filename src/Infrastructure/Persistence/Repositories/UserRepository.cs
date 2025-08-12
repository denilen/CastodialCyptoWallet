using System.Data;
using CryptoWallet.Domain.Users;
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

        // Начинаем транзакцию
        await using var transaction =
            await Context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            // Проверяем, существует ли пользователь с таким email
            if (await ExistsWithEmailAsync(user.Email, cancellationToken))
            {
                throw new InvalidOperationException($"User with email '{user.Email}' already exists.");
            }

            // Добавляем пользователя
            await DbSet.AddAsync(user, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);

            // Получаем все активные криптовалюты
            var cryptocurrencies = await Context.Cryptocurrencies
                .Where(c => c.IsActive)
                .ToListAsync(cancellationToken);

            if (!cryptocurrencies.Any())
            {
                throw new InvalidOperationException("No active cryptocurrencies found to create wallets.");
            }

            // Создаем кошельки для каждой криптовалюты
            var wallets = new List<Wallet>();
            foreach (var crypto in cryptocurrencies)
            {
                // Генерируем уникальный адрес кошелька
                var walletAddress = GenerateWalletAddress(crypto.Code, user.Id);

                wallets.Add(new Wallet(user, crypto, walletAddress));
            }

            // Добавляем кошельки
            await Context.Wallets.AddRangeAsync(wallets, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);

            // Фиксируем транзакцию
            await transaction.CommitAsync(cancellationToken);

            // Возвращаем пользователя с загруженными кошельками
            return await DbSet
                .Include(u => u.Wallets)
                .ThenInclude(w => w.Cryptocurrency)
                .FirstAsync(u => u.Id == user.Id, cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static string GenerateWalletAddress(string cryptoCode, Guid userId)
    {
        // In the real application, there should be a more complex logic of the address of the address
        // Given the specifics of the blockchain of the selected cryptocurrency
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = new Random().Next(1000, 9999);
        return $"{cryptoCode.ToLower()}_{userId.ToString("N")[..8]}_{timestamp}_{random}";
    }
}
