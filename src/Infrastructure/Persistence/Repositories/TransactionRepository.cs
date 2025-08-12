using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Enums;
using CryptoWallet.Domain.Transactions;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;
using CryptoWallet.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace CryptoWallet.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation of the transaction repository
/// </summary>
public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<PaginatedList<Transaction>> GetWalletTransactionsAsync(
        Wallet wallet,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (wallet == null)
            throw new ArgumentNullException(nameof(wallet));

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");

        var query = DbSet
            .AsNoTracking()
            .Include(t => t.Wallet)
            .ThenInclude(w => w.Cryptocurrency)
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt);

        return await PaginatedList<Transaction>.CreateAsync(query, pageNumber, pageSize, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaginatedList<Transaction>> GetUserTransactionsAsync(
        User user,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");

        var query = DbSet
            .AsNoTracking()
            .Include(t => t.Wallet)
            .ThenInclude(w => w.Cryptocurrency)
            .Where(t => t.Wallet.UserId == user.Id)
            .OrderByDescending(t => t.CreatedAt);

        return await PaginatedList<Transaction>.CreateAsync(query, pageNumber, pageSize, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Transaction?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(t => t.Wallet)
            .ThenInclude(w => w.Cryptocurrency)
            .Include(t => t.Wallet)
            .ThenInclude(w => w.User)
            .Include(t => t.RelatedTransaction)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Transaction?> GetByTransactionHashAsync(
        string transactionHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transactionHash))
            throw new ArgumentException("Transaction hash cannot be null or whitespace.", nameof(transactionHash));

        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TransactionHash == transactionHash, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaginatedList<Transaction>> GetByStatusAsync(
        TransactionStatusEnum statusEnum,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");

        var query = DbSet
            .AsNoTracking()
            .Include(t => t.Wallet)
            .ThenInclude(w => w.Cryptocurrency)
            .Where(t => t.StatusEnum == statusEnum)
            .OrderBy(t => t.CreatedAt);

        return await PaginatedList<Transaction>.CreateAsync(query, pageNumber, pageSize, cancellationToken);
    }

    /// <summary>
    /// Get transaction by ID with included details
    /// </summary>
    public override async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get all transactions with included details
    /// </summary>
    public override IQueryable<Transaction> GetAll()
    {
        return base.GetAll()
            .Include(t => t.Wallet)
            .ThenInclude(w => w.Cryptocurrency)
            .Include(t => t.Wallet)
            .ThenInclude(w => w.User)
            .AsQueryable();
    }
}
