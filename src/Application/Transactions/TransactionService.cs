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

            // Input validation
            if (string.IsNullOrWhiteSpace(walletAddress))
                return Result.Error("Wallet address cannot be empty.");

            if (!IsValidWalletAddress(walletAddress))
                return Result.Error("Invalid wallet address format.");

            if (pageNumber < 1)
                return Result.Error("Page number must be greater than zero.");

            if (pageSize < 1 || pageSize > 100)
                return Result.Error("Page size must be between 1 and 100.");

            // Check if wallet exists and is active
            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(walletAddress, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet with address {WalletAddress} not found", walletAddress);
                return Result.NotFound($"Wallet with address '{walletAddress}' not found.");
            }

            if (!wallet.IsActive)
            {
                _logger.LogWarning("Wallet with address {WalletAddress} is not active", walletAddress);
                return Result.Error("This wallet is not active and cannot perform transactions.");
            }

            // Build query with additional filtering options
            var query = _transactionRepository.GetQueryable()
                .Where(t => t.Wallet.Address == walletAddress)
                .OrderByDescending(t => t.CreatedAt);

            var transactionCount = await query.CountAsync(cancellationToken);
            if (transactionCount == 0)
            {
                _logger.LogInformation("No transactions found for wallet: {WalletAddress}", walletAddress);
                return Result.Success(new PaginatedList<TransactionDto>([], 0, pageNumber, pageSize));
            }

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} transactions for wallet: {WalletAddress}", 
                transactions.Items.Count, walletAddress);

            return Result.Success(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching transactions for wallet: {WalletAddress}",
                walletAddress);
            return Result.Error("An unexpected error occurred while retrieving transactions. Please try again later.");
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
        string? walletAddress = null,
        string? status = null,
        string? type = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching transactions between {StartDate} and {EndDate}", startDate, endDate);

            // Input validation
            if (startDate > endDate)
                return Result.Error("Start date cannot be after end date.");

            var maxDateRangeInDays = 365; // 1 year
            if ((endDate - startDate).TotalDays > maxDateRangeInDays)
                return Result.Error($"Date range cannot exceed {maxDateRangeInDays} days.");

            if (startDate > DateTimeOffset.UtcNow)
                return Result.Error("Start date cannot be in the future.");

            if (pageNumber < 1)
                return Result.Error("Page number must be greater than zero.");

            if (pageSize < 1 || pageSize > 100)
                return Result.Error("Page size must be between 1 and 100.");

            // Cap the end date to current time if it's in the future
            var effectiveEndDate = endDate > DateTimeOffset.UtcNow ? DateTimeOffset.UtcNow : endDate;

            // Build query with filters
            var query = _transactionRepository.GetQueryable()
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= effectiveEndDate);

            // Apply additional filters if provided
            if (!string.IsNullOrWhiteSpace(walletAddress))
            {
                if (!IsValidWalletAddress(walletAddress))
                    return Result.Error("Invalid wallet address format.");
                
                query = query.Where(t => t.Wallet.Address == walletAddress);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToLowerInvariant();
                query = query.Where(t => t.Status.ToLower() == normalizedStatus);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                var normalizedType = type.Trim().ToLowerInvariant();
                query = query.Where(t => t.Type.ToLower() == normalizedType);
            }

            if (minAmount.HasValue && minAmount > 0)
            {
                query = query.Where(t => t.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue && maxAmount > 0)
            {
                if (minAmount.HasValue && maxAmount < minAmount)
                    return Result.Error("Maximum amount cannot be less than minimum amount.");
                    
                query = query.Where(t => t.Amount <= maxAmount.Value);
            }

            // Order and paginate
            query = query.OrderByDescending(t => t.CreatedAt);

            var transactionCount = await query.CountAsync(cancellationToken);
            if (transactionCount == 0)
            {
                _logger.LogInformation("No transactions found for the specified criteria");
                return Result.Success(new PaginatedList<TransactionDto>([], 0, pageNumber, pageSize));
            }

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} transactions for the specified criteria", 
                transactions.Items.Count);

            return Result.Success(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching transactions between {StartDate} and {EndDate}",
                startDate, endDate);
            return Result.Error("An unexpected error occurred while retrieving transactions. Please try again later.");
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
            if (!IsValidStatusTransition(transaction.StatusEnum, status))
            {
                _logger.LogWarning(
                    "Invalid status transition from {OldStatus} to {NewStatus} for transaction {TransactionId}",
                    transaction.StatusEnum, status, transactionId);
                return Result.Error($"Cannot change status from '{transaction.StatusEnum}' to '{status}'.");
            }

            // Update transaction status
            var previousStatus = transaction.StatusEnum;
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

    private bool IsValidWalletAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        // Basic length check (adjust based on your cryptocurrency requirements)
        if (address.Length < 26 || address.Length > 64)
            return false;

        // Check for valid characters (alphanumeric, but depends on the cryptocurrency)
        // This is a simplified example - you should implement specific validation for each cryptocurrency
        return System.Text.RegularExpressions.Regex.IsMatch(address, "^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$|^0x[a-fA-F0-9]{40}$");
    }

    private bool IsValidTransactionType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return false;

        // Define valid transaction types
        var validTypes = new[] 
        { 
            "deposit", 
            "withdrawal", 
            "transfer_in", 
            "transfer_out",
            "exchange"
        };

        return validTypes.Contains(type.ToLowerInvariant());
    }

    private bool IsValidTransactionStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        // Define valid transaction statuses
        var validStatuses = new[] 
        { 
            "pending",
            "processing",
            "completed",
            "failed",
            "cancelled"
        };

        return validStatuses.Contains(status.ToLowerInvariant());
    }

    private bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        // Normalize the statuses for comparison
        currentStatus = currentStatus?.Trim().ToLowerInvariant() ?? string.Empty;
        newStatus = newStatus?.Trim().ToLowerInvariant() ?? string.Empty;

        // If the statuses are the same, it's always a valid transition (idempotent operation)
        if (currentStatus == newStatus)
            return true;

        // Define valid status transitions with more granular control
        var validTransitions = new Dictionary<string, HashSet<string>>
        {
            // Pending can go to processing, completed, failed, or cancelled
            ["pending"] = new HashSet<string> 
            { 
                "processing",  // Transaction is being processed
                "completed",   // Direct completion for simple transactions
                "failed",      // Transaction failed
                "cancelled"    // User or system cancelled the transaction
            },

            // Processing can go to completed or failed
            ["processing"] = new HashSet<string> 
            { 
                "completed",   // Successfully processed
                "failed",      // Processing failed
                "cancelled"    // User or system cancelled during processing
            },

            // Completed is a terminal state (no further transitions allowed)
            ["completed"] = new HashSet<string>(),
            
            // Failed can be retried (goes back to pending)
            ["failed"] = new HashSet<string> 
            { 
                "pending"      // Allow retrying failed transactions
            },

            // Cancelled is a terminal state (no further transitions allowed)
            ["cancelled"] = new HashSet<string>()
        };

        // If the current status isn't in our dictionary, log a warning but allow the transition
        if (!validTransitions.ContainsKey(currentStatus))
        {
            _logger.LogWarning("Unknown current status '{CurrentStatus}' during status transition validation. " +
                             "Allowing transition to '{NewStatus}' but this should be reviewed.", 
                             currentStatus, newStatus);
            return true;
        }

        // Check if the transition is valid
        var isValid = validTransitions[currentStatus].Contains(newStatus);
        
        if (!isValid)
        {
            _logger.Warning("Invalid status transition from '{CurrentStatus}' to '{NewStatus}'. " +
                          "Allowed transitions: {AllowedTransitions}",
                          currentStatus, 
                          newStatus, 
                          string.Join(", ", validTransitions[currentStatus]));
        }

        return isValid;
    }

    #endregion
}
