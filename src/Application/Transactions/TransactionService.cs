using Ardalis.Result;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CryptoWallet.Application.Common.Models;
using CryptoWallet.Application.Common.Services;
using CryptoWallet.Application.Common.Validators;
using CryptoWallet.Application.Transactions.Dtos;
using CryptoWallet.Domain.Enums;
using CryptoWallet.Domain.Interfaces.Repositories;
using CryptoWallet.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                return Result.NotFound("Transaction not found.");
            }

            var result = _mapper.Map<TransactionDto>(transaction);
            return new Result<TransactionDto>(result);
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
                return Result.NotFound("Wallet not found.");
            }

            if (!wallet.IsActive)
            {
                _logger.LogWarning("Wallet with address {WalletAddress} is not active", walletAddress);
                return Result.Error("This wallet is not active and cannot perform transactions.");
            }

            // Build query with additional filtering options
            var query = _transactionRepository.GetAll()
                .Where(t => t.Wallet.Address == walletAddress)
                .OrderByDescending(t => t.CreatedAt);

            var transactionCount = await query.CountAsync(cancellationToken);
            if (transactionCount == 0)
            {
                _logger.LogInformation("No transactions found for wallet: {WalletAddress}", walletAddress);
                return new Result<PaginatedList<TransactionDto>>(
                    new PaginatedList<TransactionDto>(new List<TransactionDto>(), 0, pageNumber, pageSize));
            }

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} transactions for wallet: {WalletAddress}",
                transactions.Items.Count, walletAddress);

            return new Result<PaginatedList<TransactionDto>>(transactions);
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

            var query = _transactionRepository.GetAll()
                .Where(t => t.Wallet.UserId == user.Id)
                .OrderByDescending(t => t.CreatedAt);

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            return new Result<PaginatedList<TransactionDto>>(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching transactions for user ID: {UserId}", user.Id);
            return Result.Error("An error occurred while fetching transactions.");
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

            if (!Enum.TryParse<TransactionStatusEnum>(status, true, out var statusEnum))
                return Result.Error($"Invalid status value: {status}");

            var query = _transactionRepository.GetAll()
                .Where(t => t.StatusEnum == statusEnum)
                .OrderByDescending(t => t.CreatedAt);

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            return new Result<PaginatedList<TransactionDto>>(transactions);
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

            var query = _transactionRepository.GetAll()
                .Where(t => t.TypeEnum.ToString().ToLower() == normalizedType)
                .OrderByDescending(t => t.CreatedAt);

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            return new Result<PaginatedList<TransactionDto>>(transactions);
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

            if (pageNumber < 1)
                return Result.Error("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                return Result.Error("Page size must be between 1 and 100.");

            if (!string.IsNullOrWhiteSpace(walletAddress) && !IsValidWalletAddress(walletAddress))
                return Result.Error("Invalid wallet address format.");

            if (!string.IsNullOrWhiteSpace(status) && !IsValidTransactionStatus(status))
                return Result.Error($"Invalid status: {status}");

            if (!string.IsNullOrWhiteSpace(type) && !IsValidTransactionType(type))
                return Result.Error($"Invalid transaction type: {type}");

            if (minAmount < 0 || maxAmount < 0)
                return Result.Error("Amount cannot be negative.");

            if (minAmount > maxAmount)
                return Result.Error("Minimum amount cannot be greater than maximum amount.");

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
            var query = _transactionRepository.GetAll()
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
                query = query.Where(t => t.StatusEnum.ToString().ToLower() == normalizedStatus);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                var normalizedType = type.Trim().ToLowerInvariant();
                query = query.Where(t => t.TypeEnum.ToString().ToLower() == normalizedType);
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
                return new Result<PaginatedList<TransactionDto>>(
                    new PaginatedList<TransactionDto>(new List<TransactionDto>(), 0, pageNumber, pageSize));
            }

            var transactions = await PaginatedList<TransactionDto>.CreateAsync(
                query.ProjectTo<TransactionDto>(_mapper.ConfigurationProvider),
                pageNumber,
                pageSize,
                cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} transactions for the specified criteria",
                transactions.Items.Count);

            return new Result<PaginatedList<TransactionDto>>(transactions);
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
        if (string.IsNullOrWhiteSpace(status))
            return Result.Error("Status cannot be empty.");

        if (transactionId == Guid.Empty)
            return Result.Error("Transaction ID cannot be empty.");

        try
        {
            _logger.LogInformation("Updating status of transaction {TransactionId} to {Status}", transactionId, status);

            var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction == null)
                return Result.NotFound("Transaction not found.");

            if (_transactionRepository == null)
            {
                _logger.LogError("Transaction repository is not initialized");
                return Result.Error("Internal server error: Transaction service is not properly initialized.");
            }

            // Validate status transition
            if (!IsValidStatusTransition(transaction.StatusEnum.ToString(), status))
                return Result.Error($"Invalid status transition from {transaction.StatusEnum} to {status}");

            // Update transaction status
            var previousStatus = transaction.StatusEnum.ToString();

            // Use the appropriate status update method based on the new status
            if (Enum.TryParse<TransactionStatusEnum>(status, true, out var newStatus))
            {
                switch (newStatus)
                {
                    case TransactionStatusEnum.Processing:
                        transaction.MarkAsProcessing();
                        break;
                    case TransactionStatusEnum.Completed:
                        transaction.MarkAsCompleted();
                        break;
                    case TransactionStatusEnum.Failed:
                        transaction.MarkAsFailed(notes ?? "Transaction failed");
                        break;
                    case TransactionStatusEnum.Cancelled:
                        transaction.MarkAsCancelled(notes);
                        break;
                    case TransactionStatusEnum.Rejected:
                        transaction.MarkAsRejected(notes ?? "Transaction rejected");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(status), $"Unsupported status: {status}");
                }
            }
            else
            {
                _logger.LogWarning("Invalid status value: {Status}", status);
                return Result.Error($"Invalid status value: {status}");
            }

            // Save the changes
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully updated status of transaction {TransactionId} from {OldStatus} to {NewStatus}",
                transactionId, previousStatus, status);

            // Reload the transaction with details to include related data
            var updatedTransaction =
                await _transactionRepository.GetByIdWithDetailsAsync(transactionId, cancellationToken);

            if (updatedTransaction == null)
            {
                _logger.LogError("Failed to reload transaction {TransactionId} after status update", transactionId);
                return Result.Error("Transaction was updated but could not be reloaded.");
            }

            if (_mapper == null)
            {
                _logger.LogError("AutoMapper is not initialized");
                return Result.Error("Internal server error: Mapping service is not properly initialized.");
            }

            var result = _mapper.Map<TransactionDto>(updatedTransaction);
            return new Result<TransactionDto>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating status of transaction {TransactionId}", transactionId);
            return Result.Error("An error occurred while updating the transaction status.");
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Validates a cryptocurrency wallet address using the WalletAddressValidator
    /// </summary>
    /// <param name="address">The wallet address to validate</param>
    /// <returns>True if the address is valid, false otherwise</returns>
    private bool IsValidWalletAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            _logger.LogWarning("Wallet address is null or empty");
            return false;
        }

        // Use the WalletAddressValidator for consistent validation
        bool isValid = address.IsValidWalletAddress();

        if (!isValid)
        {
            _logger.LogWarning("Invalid wallet address format: {Address}", address);
        }

        return isValid;
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
        return Enum.TryParse<TransactionStatusEnum>(status, true, out _);
    }

    private bool IsValidStatusTransition(string currentStatus, string newStatus)
    {
        // Parse the status enums
        if (!Enum.TryParse<TransactionStatusEnum>(currentStatus, true, out var currentStatusEnum) ||
            !Enum.TryParse<TransactionStatusEnum>(newStatus, true, out var newStatusEnum))
        {
            _logger.LogWarning("Invalid status values: Current='{CurrentStatus}', New='{NewStatus}'", currentStatus,
                newStatus);
            return false;
        }

        // If the statuses are the same, it's always a valid transition (idempotent operation)
        if (currentStatusEnum == newStatusEnum)
            return true;

        // Define valid status transitions with more granular control
        var validTransitions = new Dictionary<TransactionStatusEnum, HashSet<TransactionStatusEnum>>
        {
            // Pending can go to processing, completed, failed, or cancelled
            [TransactionStatusEnum.Pending] = new HashSet<TransactionStatusEnum>
            {
                TransactionStatusEnum.Processing, // Transaction is being processed
                TransactionStatusEnum.Completed,  // Direct completion for simple transactions
                TransactionStatusEnum.Failed,     // Transaction failed
                TransactionStatusEnum.Cancelled   // User or system cancelled the transaction
            },

            // Processing can go to completed or failed
            [TransactionStatusEnum.Processing] = new HashSet<TransactionStatusEnum>
            {
                TransactionStatusEnum.Completed, // Successfully processed
                TransactionStatusEnum.Failed,    // Processing failed
                TransactionStatusEnum.Cancelled  // User or system cancelled during processing
            },

            // Completed is a terminal state (no further transitions allowed)
            [TransactionStatusEnum.Completed] = new HashSet<TransactionStatusEnum>(),

            // Failed can be retried (goes back to pending)
            [TransactionStatusEnum.Failed] = new HashSet<TransactionStatusEnum>
            {
                TransactionStatusEnum.Pending // Allow retrying failed transactions
            },

            // Cancelled is a terminal state (no further transitions allowed)
            [TransactionStatusEnum.Cancelled] = new HashSet<TransactionStatusEnum>()
        };

        // If the current status isn't in our dictionary, log a warning but allow the transition
        if (!validTransitions.ContainsKey(currentStatusEnum))
        {
            _logger.LogWarning("Unknown current status '{CurrentStatus}' during status transition validation. " +
                               "Allowing transition to '{NewStatus}' but this should be reviewed.",
                currentStatus, newStatus);
            return true;
        }

        // Check if the transition is valid
        var allowedTransitions = validTransitions[currentStatusEnum];
        var isValid = allowedTransitions.Contains(newStatusEnum);

        if (!isValid)
        {
            _logger.LogWarning("Invalid status transition from '{CurrentStatus}' to '{NewStatus}'. " +
                               "Allowed transitions: {AllowedTransitions}",
                currentStatusEnum,
                newStatusEnum,
                string.Join(", ", allowedTransitions));
        }

        return isValid;
    }

    #endregion
}
