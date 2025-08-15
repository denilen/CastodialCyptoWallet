using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Enums;
using CryptoWallet.Domain.Interfaces.Repositories;
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
        if (wallet.Id == Guid.Empty)
            throw new ArgumentException("Wallet ID cannot be empty.", nameof(wallet));

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");
        if (pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size cannot exceed 100.");

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
        if (user.Id == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(user));

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");
        if (pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size cannot exceed 100.");

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
        if (transactionHash.Length > 128) // Adjust max length as needed
            throw new ArgumentException("Transaction hash is too long.", nameof(transactionHash));

        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TransactionHash == transactionHash, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PaginatedList<Transaction>> GetByStatusAsync(
        TransactionStatusEnum status,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(TransactionStatusEnum), status))
            throw new ArgumentException("Invalid transaction status value.", nameof(status));

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");
        if (pageSize > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size cannot exceed 100.");

        var query = DbSet
            .AsNoTracking()
            .Include(t => t.Wallet)
            .ThenInclude(w => w.Cryptocurrency)
            .Where(t => t.StatusEnum == status)
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<Transaction>> GetPendingWithdrawalsAsync(
        Guid walletId,
        CancellationToken cancellationToken = default)
    {
        if (walletId == Guid.Empty)
            throw new ArgumentException("Wallet ID cannot be empty.", nameof(walletId));

        return await DbSet
            .AsNoTracking()
            .Include(t => t.Wallet)
            .ThenInclude(w => w.Cryptocurrency)
            .Where(t => t.WalletId == walletId &&
                        t.TypeEnum == TransactionTypeEnum.Withdrawal &&
                        t.StatusEnum == TransactionStatusEnum.Pending)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<decimal> GetTotalWithdrawnAmountAsync(
        Guid userId,
        Guid cryptocurrencyId,
        DateTimeOffset fromDate,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        if (cryptocurrencyId == Guid.Empty)
            throw new ArgumentException("Cryptocurrency ID cannot be empty.", nameof(cryptocurrencyId));
        if (fromDate > DateTimeOffset.UtcNow)
            throw new ArgumentException("From date cannot be in the future.", nameof(fromDate));

        return await DbSet
            .AsNoTracking()
            .Where(t => t.Wallet.UserId == userId &&
                        t.Wallet.CryptocurrencyId == cryptocurrencyId &&
                        t.TypeEnum == TransactionTypeEnum.Withdrawal &&
                        t.StatusEnum == TransactionStatusEnum.Completed &&
                        t.CreatedAt >= fromDate)
            .SumAsync(t => t.Amount, cancellationToken);
    }
}
