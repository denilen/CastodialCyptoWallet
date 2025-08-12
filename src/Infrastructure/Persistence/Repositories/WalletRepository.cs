using CryptoWallet.Domain.Currencies;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;
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

        if (cryptocurrency == null)
            throw new ArgumentNullException(nameof(cryptocurrency));

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
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or whitespace.", nameof(address));

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
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be null or whitespace.", nameof(address));

        return await DbSet
            .AsNoTracking()
            .AnyAsync(w => w.Address == address, cancellationToken);
    }

    /// <summary>
    /// Get wallet by ID with included user and cryptocurrency details
    /// </summary>
    public override async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(w => w.User)
            .Include(w => w.Cryptocurrency)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get all wallets with included details
    /// </summary>
    public override IQueryable<Wallet> GetAll()
    {
        return base.GetAll()
            .Include(w => w.User)
            .Include(w => w.Cryptocurrency)
            .AsQueryable();
    }
}
