using CryptoWallet.Application.Common.Models;
using CryptoWallet.Application.Common.Services;
using CryptoWallet.Application.Transactions.Dtos;
using CryptoWallet.Domain.Transactions;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;

namespace CryptoWallet.Application.Transactions;

/// <summary>
/// Provides services for managing cryptocurrency transactions
/// </summary>
public class TransactionService : BaseService, ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ILogger<TransactionService> logger,
        ITransactionRepository transactionRepository,
        IWalletRepository walletRepository,
        IMapper mapper)
        : base(logger)
    {
        _transactionRepository =
            transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<TransactionDto>> GetTransactionByIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching transaction with ID: {TransactionId}", transactionId);

            var transaction = await _transactionRepository.GetByIdWithDetailsAsync(transactionId, cancellationToken);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction with ID {TransactionId} not found", transactionId);
                return Result.NotFound($"Transaction with ID '{transactionId}' not found.");
            }

            var result = _mapper.Map<TransactionDto>(transaction);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching transaction with ID: {TransactionId}", transactionId);
            return Result.Error($"An error occurred while fetching transaction with ID '{transactionId}'.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<PaginatedList<TransactionDto>>> GetWalletTransactionsAsync(
        string walletAddress,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching transactions for wallet: {WalletAddress}", walletAddress);

            if (string.IsNullOrWhiteSpace(walletAddress))
                return Result.Error("Wallet address cannot be empty.");

            // Check if wallet exists
            var walletExists = await _walletRepository.ExistsByAddressAsync(walletAddress, cancellationToken);
            if (!walletExists)
            {
                _logger.LogWarning("Wallet with address {WalletAddress} not found", walletAddress);
                return Result.NotFound($"Wallet with address '{walletAddress}' not found.");
            }

            var query = _transactionRepository.GetQueryable()
                .Where(t => t.Wallet.Address == walletAddress)
                .OrderByDescending(t => t.CreatedAt);

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            return Result.Success(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching transactions for wallet: {WalletAddress}",
                walletAddress);
            return Result.Error($"An error occurred while fetching transactions for wallet '{walletAddress}'.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<PaginatedList<TransactionDto>>> GetUserTransactionsAsync(
        User user,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all transactions for user ID: {UserId}", user.Id);

            var query = _transactionRepository.GetQueryable()
                .Where(t => t.Wallet.UserId == user.Id)
                .OrderByDescending(t => t.CreatedAt);

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            return Result.Success(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching transactions for user ID: {UserId}", user.Id);
            return Result.Error($"An error occurred while fetching transactions for user ID '{user.Id}'.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<PaginatedList<TransactionDto>>> GetTransactionsByStatusAsync(
        string status,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching transactions with status: {Status}", status);

            if (string.IsNullOrWhiteSpace(status))
                return Result.Error("Status cannot be empty.");

            var normalizedStatus = status.Trim().ToLowerInvariant();

            var query = _transactionRepository.GetQueryable()
                .Where(t => t.Status.ToLower() == normalizedStatus)
                .OrderByDescending(t => t.CreatedAt);

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            return Result.Success(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching transactions with status: {Status}", status);
            return Result.Error($"An error occurred while fetching transactions with status '{status}'.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<PaginatedList<TransactionDto>>> GetTransactionsByTypeAsync(
        string type,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching transactions with type: {Type}", type);

            if (string.IsNullOrWhiteSpace(type))
                return Result.Error("Type cannot be empty.");

            var normalizedType = type.Trim().ToLowerInvariant();

            var query = _transactionRepository.GetQueryable()
                .Where(t => t.Type.ToLower() == normalizedType)
                .OrderByDescending(t => t.CreatedAt);

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            return Result.Success(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching transactions with type: {Type}", type);
            return Result.Error($"An error occurred while fetching transactions with type '{type}'.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<PaginatedList<TransactionDto>>> GetTransactionsByDateRangeAsync(
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching transactions between {StartDate} and {EndDate}", startDate, endDate);

            if (startDate > endDate)
                return Result.Error("Start date cannot be after end date.");

            if (startDate > DateTimeOffset.UtcNow)
                return Result.Error("Start date cannot be in the future.");

            // Cap the end date to current time if it's in the future
            var effectiveEndDate = endDate > DateTimeOffset.UtcNow ? DateTimeOffset.UtcNow : endDate;

            var query = _transactionRepository.GetQueryable()
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= effectiveEndDate)
                .OrderByDescending(t => t.CreatedAt);

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            return Result.Success(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching transactions between {StartDate} and {EndDate}",
                startDate, endDate);
            return Result.Error($"An error occurred while fetching transactions for the specified date range.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<TransactionDto>> UpdateTransactionStatusAsync(
        Guid transactionId,
        string status,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating status of transaction {TransactionId} to {Status}", transactionId, status);

            if (string.IsNullOrWhiteSpace(status))
                return Result.Error("Status cannot be empty.");

            var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction with ID {TransactionId} not found", transactionId);
                return Result.NotFound($"Transaction with ID '{transactionId}' not found.");
            }

            // Validate status transition
            if (!IsValidStatusTransition(transaction.Status, status))
            {
                _logger.LogWarning(
                    "Invalid status transition from {OldStatus} to {NewStatus} for transaction {TransactionId}",
                    transaction.Status, status, transactionId);
                return Result.Error($"Cannot change status from '{transaction.Status}' to '{status}'.");
            }

            // Update transaction status
            var previousStatus = transaction.Status;
            transaction.UpdateStatus(status, notes);

            // If the transaction is now completed, update the confirmed timestamp
            if (status.Equals("completed", StringComparison.OrdinalIgnoreCase))
            {
                transaction.MarkAsCompleted();
            }

            await _transactionRepository.UpdateAsync(transaction, cancellationToken);
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully updated status of transaction {TransactionId} from {OldStatus} to {NewStatus}",
                transactionId, previousStatus, status);

            var result = _mapper.Map<TransactionDto>(transaction);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating status of transaction {TransactionId}", transactionId);
            return Result.Error($"An error occurred while updating the status of transaction '{transactionId}'.");
        }
    }

    #region Private Helper Methods

    private bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        // Normalize the statuses for comparison
        currentStatus = currentStatus?.Trim().ToLowerInvariant() ?? string.Empty;
        newStatus = newStatus?.Trim().ToLowerInvariant() ?? string.Empty;

        // Define valid status transitions
        var validTransitions = new Dictionary<string, HashSet<string>>
        {
            // Pending can go to processing, completed, or failed
            ["pending"] = new HashSet<string> { "processing", "completed", "failed" },

            // Processing can go to completed or failed
            ["processing"] = new HashSet<string> { "completed", "failed" },

            // Completed and failed are terminal states
            ["completed"] = new HashSet<string>(),
            ["failed"] = new HashSet<string>(),

            // Cancelled is also a terminal state
            ["cancelled"] = new HashSet<string>()
        };

        // If the current status isn't in our dictionary, allow the transition (we don't know any better)
        if (!validTransitions.ContainsKey(currentStatus))
            return true;

        // If the new status is the same as the current status, it's always valid
        if (currentStatus == newStatus)
            return true;

        // Check if the transition is valid
        return validTransitions[currentStatus].Contains(newStatus);
    }

    #endregion
}
