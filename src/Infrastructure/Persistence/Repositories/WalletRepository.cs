using CryptoWallet.Domain.Currencies;
using CryptoWallet.Domain.Interfaces.Repositories;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;
using CryptoWallet.Infrastructure.Extensions;
using CryptoWallet.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace CryptoWallet.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation of the wallet repository
/// </summary>
public class WalletRepository : Repository<Wallet>, IWalletRepository
{
    public WalletRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Wallet?> GetUserWalletByCurrencyAsync(
        User user,
        Cryptocurrency cryptocurrency,
        CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        if (user.Id == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(user));

        if (cryptocurrency == null)
            throw new ArgumentNullException(nameof(cryptocurrency));
        if (cryptocurrency.Id == Guid.Empty)
            throw new ArgumentException("Cryptocurrency ID cannot be empty.", nameof(cryptocurrency));

        return await DbSet
            .AsNoTracking()
            .Include(w => w.Cryptocurrency)
            .FirstOrDefaultAsync(
                w => w.UserId == user.Id &&
                     w.CryptocurrencyId == cryptocurrency.Id,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Wallet>> GetUserWalletsWithDetailsAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        if (user.Id == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(user));

        return await DbSet
            .AsNoTracking()
            .Include(w => w.Cryptocurrency)
            .Where(w => w.UserId == user.Id)
            .OrderBy(w => w.Cryptocurrency.Code)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Wallet?> GetByAddressWithDetailsAsync(
        string address,
        CancellationToken cancellationToken = default)
    {
        address.EnsureValidWalletAddress(nameof(address));

        return await DbSet
            .AsNoTracking()
            .Include(w => w.User)
            .Include(w => w.Cryptocurrency)
            .FirstOrDefaultAsync(w => w.Address == address, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsWithAddressAsync(
        string address,
        CancellationToken cancellationToken = default)
    {
        address.EnsureValidWalletAddress(nameof(address));

        return await DbSet
            .AsNoTracking()
            .AnyAsync(w => w.Address == address, cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<Wallet?> GetByAddressAsync(
        string address,
        CancellationToken cancellationToken = default)
    {
        address.EnsureValidWalletAddress(nameof(address));

        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Address == address, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Wallet?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty.", nameof(id));
            
        return await DbSet
            .AsNoTracking()
            .Include(w => w.User)
            .Include(w => w.Cryptocurrency)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Wallet>> GetUserWalletsByIdWithDetailsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return await DbSet
            .AsNoTracking()
            .Include(w => w.Cryptocurrency)
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.Cryptocurrency.Code)
            .ToListAsync(cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task<Wallet?> GetUserWalletByCurrencyWithDetailsAsync(
        Guid userId,
        string currencyCode,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code cannot be empty.", nameof(currencyCode));

        return await DbSet
            .AsNoTracking()
            .Include(w => w.Cryptocurrency)
            .FirstOrDefaultAsync(
                w => w.UserId == userId && 
                     w.Cryptocurrency.Code == currencyCode.ToUpper(),
                cancellationToken);
    }

    /// <inheritdoc />
    public override IQueryable<Wallet> GetAll()
    {
        return base.GetAll()
            .Include(w => w.User)
            .Include(w => w.Cryptocurrency);
    }
}
