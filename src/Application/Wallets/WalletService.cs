using Ardalis.Result;
using AutoMapper;
using CryptoWallet.Application.Common.Interfaces;
using CryptoWallet.Application.Common.Services;
using CryptoWallet.Application.Common.Validators;
using CryptoWallet.Domain.Currencies;
using CryptoWallet.Domain.Enums;
using CryptoWallet.Domain.Interfaces.Repositories;
using CryptoWallet.Domain.Models.DTOs.Wallets;
using CryptoWallet.Domain.Transactions;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.Application.Wallets;

/// <summary>
/// Service for wallet management operations
/// </summary>
public class WalletService : BaseService, IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICryptocurrencyRepository _cryptocurrencyRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        ILogger<WalletService> logger,
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        ICryptocurrencyRepository cryptocurrencyRepository,
        IMapper mapper)
        : base(logger)
    {
        _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
        _transactionRepository =
            transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _cryptocurrencyRepository = cryptocurrencyRepository ??
                                    throw new ArgumentNullException(nameof(cryptocurrencyRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Result<WalletDto>> GetWalletByAddressAsync(
        string address,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching wallet by address: {Address}", address);

            if (string.IsNullOrWhiteSpace(address))
                return Result.Error("Wallet address cannot be empty.");

            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(address, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet with address {Address} not found", address);
                return Result.NotFound($"Wallet with address '{address}' not found.");
            }

            var result = _mapper.Map<WalletDto>(wallet);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching wallet with address: {Address}", address);
            return Result.Error($"An error occurred while fetching wallet with address '{address}'.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<WalletDto>>> GetUserWalletsAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all wallets for user ID: {UserId}", user.Id);

            var wallets = await _walletRepository.GetUserWalletsWithDetailsAsync(user, cancellationToken);
            var result = _mapper.Map<IReadOnlyList<WalletDto>>(wallets);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching wallets for user ID: {UserId}", user.Id);
            return Result.Error("An error occurred while fetching wallets.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<WalletDto>> GetUserWalletByCurrencyAsync(
        User user,
        string currencyCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching {Currency} wallet for user ID: {UserId}", currencyCode, user.Id);

            if (string.IsNullOrWhiteSpace(currencyCode))
                return Result.Error("Currency code cannot be empty.");

            var cryptocurrency = await _cryptocurrencyRepository.GetByCodeAsync(currencyCode, cancellationToken);
            if (cryptocurrency == null)
            {
                _logger.LogWarning("Cryptocurrency with code {CurrencyCode} not found", currencyCode);
                return Result.NotFound($"Cryptocurrency with code '{currencyCode}' not found.");
            }

            var wallet = await _walletRepository.GetUserWalletByCurrencyAsync(user, cryptocurrency, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("No {Currency} wallet found for user ID: {UserId}", currencyCode, user.Id);
                return Result.NotFound($"No {currencyCode} wallet found for user.");
            }

            var result = _mapper.Map<WalletDto>(wallet);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching {Currency} wallet for user ID: {UserId}", currencyCode,
                user.Id);
            return Result.Error($"An error occurred while fetching {currencyCode} wallet.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<WalletBalanceDto>> GetWalletBalanceAsync(
        string walletAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching balance for wallet: {WalletAddress}", walletAddress);

            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(walletAddress, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet with address {WalletAddress} not found", walletAddress);
                return Result.NotFound($"Wallet with address '{walletAddress}' not found.");
            }

            // Calculate locked balance (sum of amounts in pending withdrawal transactions)
            var pendingWithdrawals =
                await _transactionRepository.GetPendingWithdrawalsAsync(wallet.Id, cancellationToken);
            var lockedBalance = pendingWithdrawals.Sum(t => t.Amount);

            var result = new WalletBalanceDto
            {
                Address = wallet.Address,
                CurrencyCode = wallet.Cryptocurrency.Code,
                AvailableBalance = wallet.Balance - lockedBalance,
                LockedBalance = lockedBalance,
                LastUpdated = DateTimeOffset.UtcNow
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching balance for wallet: {WalletAddress}", walletAddress);
            return Result.Error($"An error occurred while fetching balance for wallet '{walletAddress}'.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<TransactionDto>> DepositFundsAsync(
        DepositRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing deposit to wallet {WalletAddress}", request.WalletAddress);

            // Validate request
            var validationResult = await ValidateDepositRequestAsync(request, cancellationToken);
            if (!validationResult.IsSuccess)
                return validationResult;

            // Get wallet with details
            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(request.WalletAddress, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet with address {WalletAddress} not found", request.WalletAddress);
                return Result.NotFound($"Wallet with address '{request.WalletAddress}' not found.");
            }

            try
            {
                // Get the fee from the request
                decimal fee = request.Fee;
                
                // Create deposit transaction
                var depositTransaction = new Transaction(
                    wallet,
                    TransactionTypeEnum.Deposit,
                    request.Amount,
                    fee,
                    wallet.Cryptocurrency.Code,
                    request.WalletAddress,
                    request.TransactionHash,
                    request.Notes ?? $"Deposit to {request.WalletAddress}");

                // Mark transaction as processing
                depositTransaction.MarkAsProcessing();

                // Update wallet balance using domain method
                try
                {
                    wallet.Deposit(request.Amount);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Failed to update wallet balance during deposit");
                    return Result.Error("Failed to update wallet balance.");
                }

                try
                {
                    // Save changes in a transaction
                    await _transactionRepository.AddAsync(depositTransaction, cancellationToken);
                    await _walletRepository.SaveChangesAsync(cancellationToken);

                    // Mark transaction as completed
                    depositTransaction.MarkAsCompleted();
                    await _transactionRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Successfully deposited {Amount} {Currency} to wallet {WalletAddress}",
                        request.Amount, 
                        wallet.Cryptocurrency.Code,
                        request.WalletAddress);

                    return Result.Success(_mapper.Map<TransactionDto>(depositTransaction));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process deposit to wallet {WalletAddress}", request.WalletAddress);
                    return Result.Error($"Failed to process deposit: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while depositing to wallet {WalletAddress}", request.WalletAddress);
                return Result.Error($"An error occurred while processing the deposit: {ex.Message}");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while depositing to wallet {WalletAddress}", request.WalletAddress);
            return Result.Error($"An error occurred while processing the deposit to wallet '{request.WalletAddress}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<TransactionDto>> WithdrawFundsAsync(
        WithdrawRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing withdrawal from wallet {WalletAddress}", request.SourceWalletAddress);

            // Validate request
            var validationResult = await ValidateWithdrawalRequestAsync(request, cancellationToken);
            if (!validationResult.IsSuccess)
                return validationResult;

            // Get wallet with details
            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(request.SourceWalletAddress, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet with address {WalletAddress} not found", request.SourceWalletAddress);
                return Result.NotFound($"Wallet with address '{request.SourceWalletAddress}' not found.");
            }

            // Check if wallet has sufficient balance (including fee)
            var balanceResult = await GetWalletBalanceAsync(request.SourceWalletAddress, cancellationToken);
            if (!balanceResult.IsSuccess)
                return Result<TransactionDto>.Error(string.Join("; ", balanceResult.Errors));

            var availableBalance = balanceResult.Value.AvailableBalance;
            
            // Check daily withdrawal limit
            var maxDailyWithdrawal = 10000m; // Example: 10,000 USD equivalent
            var dailyWithdrawals = await _transactionRepository.GetTotalWithdrawnAmountAsync(
                wallet.UserId,
                wallet.CryptocurrencyId,
                DateTimeOffset.UtcNow.AddDays(-1),
                cancellationToken);

            if (dailyWithdrawals + request.Amount > maxDailyWithdrawal)
            {
                var currencyCode = wallet.Cryptocurrency?.Code ?? "crypto";
                return Result.Error($"Daily withdrawal limit of {maxDailyWithdrawal} {currencyCode} exceeded.");
            }

            // Validate fee
            decimal fee = request.Fee ?? 0m;
            if (fee < 0)
                return Result.Error("Fee cannot be negative.");

            if (availableBalance < request.Amount + fee)
                return Result.Error("Insufficient balance to complete the withdrawal.");

            // Validate IP address and user agent for security
            if (!string.IsNullOrWhiteSpace(request.IpAddress) && !IsValidIpAddress(request.IpAddress))
                return Result.Error("Invalid IP address format.");

            if (string.IsNullOrWhiteSpace(request.UserAgent))
                return Result.Error("User-Agent header is required for security reasons.");

            // Create withdrawal transaction
            if (wallet.Cryptocurrency == null)
            {
                _logger.LogError("Wallet {WalletAddress} has no associated cryptocurrency", wallet.Address);
                return Result.Error("Internal error: Wallet has no associated cryptocurrency.");
            }

            var withdrawalTransaction = new Transaction(
                wallet,
                TransactionTypeEnum.Withdrawal,
                request.Amount,
                fee,
                wallet.Cryptocurrency.Code,
                request.SourceWalletAddress,
                request.DestinationAddress,
                request.Notes);

            // Mark transaction as processing
            withdrawalTransaction.MarkAsProcessing();

            try
            {
                // Withdraw from wallet (includes fee)
                wallet.Withdraw(request.Amount + (request.Fee ?? 0));
                
                // Save transaction and update wallet
                await _transactionRepository.AddAsync(withdrawalTransaction, cancellationToken);
                await _walletRepository.SaveChangesAsync(cancellationToken);

                // Mark transaction as completed
                withdrawalTransaction.MarkAsCompleted();
                await _transactionRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully withdrew {Amount} {Currency} from wallet {WalletAddress} to {DestinationAddress}",
                    request.Amount, 
                    wallet.Cryptocurrency.Code,
                    request.SourceWalletAddress,
                    request.DestinationAddress);

                return Result.Success(_mapper.Map<TransactionDto>(withdrawalTransaction));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to process withdrawal from wallet {WalletAddress}", request.SourceWalletAddress);
                return Result.Error("Failed to process withdrawal: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing withdrawal from wallet {WalletAddress}", request.SourceWalletAddress);
                return Result.Error($"An unexpected error occurred while processing the withdrawal: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing withdrawal from wallet {WalletAddress}", request.SourceWalletAddress);
            return Result.Error($"An error occurred while processing the withdrawal from wallet '{request.SourceWalletAddress}': {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<Result<TransactionDto>> TransferFundsAsync(
        TransferRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initiating transfer from {SourceWallet} to {DestinationWallet}",
                request.SourceWalletAddress, request.DestinationWalletAddress);

            // Validate request
            var validationResult = await ValidateTransferRequestAsync(request, cancellationToken);
            if (!validationResult.IsSuccess)
                return validationResult;

            // Get source wallet with details
            var sourceWallet = await _walletRepository.GetByAddressWithDetailsAsync(
                request.SourceWalletAddress, cancellationToken);
            if (sourceWallet == null)
            {
                _logger.LogWarning("Source wallet with address {WalletAddress} not found", 
                    request.SourceWalletAddress);
                return Result.NotFound($"Source wallet with address '{request.SourceWalletAddress}' not found.");
            }

            // Check if source wallet has sufficient balance (including fee)
            var balanceResult = await GetWalletBalanceAsync(request.SourceWalletAddress, cancellationToken);
            if (!balanceResult.IsSuccess)
                return Result<TransactionDto>.Error(string.Join("; ", balanceResult.Errors));

            var availableBalance = balanceResult.Value.AvailableBalance;
            if (availableBalance < request.Amount + (request.Fee ?? 0))
                return Result.Error("Insufficient balance to complete the transfer.");

            // Get or create destination wallet
            var destinationWallet = await _walletRepository.GetByAddressWithDetailsAsync(
                request.DestinationWalletAddress, cancellationToken);

            if (destinationWallet == null)
            {
                // In a real application, you might want to handle this differently
                // For example, you might want to create a new wallet or return an error
                _logger.LogWarning("Destination wallet with address {WalletAddress} not found",
                    request.DestinationWalletAddress);
                return Result.NotFound($"Destination wallet with address '{request.DestinationWalletAddress}' not found.");
            }

            // Ensure both wallets are for the same cryptocurrency
            if (sourceWallet.CryptocurrencyId != destinationWallet.CryptocurrencyId)
                return Result.Error("Cannot transfer between different cryptocurrencies.");

            // Create transfer transactions (outgoing and incoming)
            var transferOutTransaction = new Transaction(
                sourceWallet,
                TransactionTypeEnum.Transfer,
                request.Amount,
                request.Fee ?? 0,
                sourceWallet.Cryptocurrency.Code,
                request.SourceWalletAddress,
                request.DestinationWalletAddress,
                request.Notes ?? $"Transfer to {request.DestinationWalletAddress}");

            var transferInTransaction = new Transaction(
                destinationWallet,
                TransactionTypeEnum.Transfer,
                request.Amount,
                0, // No fee for the recipient
                sourceWallet.Cryptocurrency.Code,
                request.SourceWalletAddress,
                request.DestinationWalletAddress,
                request.Notes ?? $"Transfer from {request.SourceWalletAddress}");

            // Link the transactions
            transferOutTransaction.LinkToRelatedTransaction(transferInTransaction);
            transferInTransaction.LinkToRelatedTransaction(transferOutTransaction);

            // Mark transactions as processing
            transferOutTransaction.MarkAsProcessing();
            transferInTransaction.MarkAsProcessing();

            try
            {
                // Withdraw from source wallet (includes fee)
                sourceWallet.Withdraw(request.Amount + (request.Fee ?? 0));
                
                // Deposit to destination wallet (amount only, no fee)
                destinationWallet.Deposit(request.Amount);
                
                // Add transactions and save changes
                await _transactionRepository.AddAsync(transferOutTransaction, cancellationToken);
                await _transactionRepository.AddAsync(transferInTransaction, cancellationToken);
                await _walletRepository.SaveChangesAsync(cancellationToken);

                // Mark transactions as completed
                transferOutTransaction.MarkAsCompleted();
                transferInTransaction.MarkAsCompleted();
                await _transactionRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully transferred {Amount} {Currency} from {SourceWallet} to {DestinationWallet}",
                    request.Amount, 
                    sourceWallet.Cryptocurrency.Code,
                    request.SourceWalletAddress,
                    request.DestinationWalletAddress);

                return Result.Success(_mapper.Map<TransactionDto>(transferOutTransaction));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to process transfer from {SourceWallet} to {DestinationWallet}",
                    request.SourceWalletAddress, request.DestinationWalletAddress);
                return Result.Error("Failed to process transfer: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing transfer from {SourceWallet} to {DestinationWallet}",
                    request.SourceWalletAddress, request.DestinationWalletAddress);
                return Result.Error($"An unexpected error occurred while processing the transfer: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in TransferFundsAsync: {Message}", ex.Message);
            return Result.Error("An unexpected error occurred while processing the transfer.");
        }
    }

    private async Task<Result> ValidateWithdrawalRequestAsync(WithdrawRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null)
                return Result.Error("Withdrawal request cannot be null.");

            // Validate wallet address
            if (string.IsNullOrWhiteSpace(request.SourceWalletAddress) || !IsValidWalletAddress(request.SourceWalletAddress))
                return Result.Error("Invalid source wallet address format.");

            // Validate destination address
            if (string.IsNullOrWhiteSpace(request.DestinationAddress) || !IsValidWalletAddress(request.DestinationAddress))
                return Result.Error("Invalid destination address format.");

            // Validate amount
            if (request.Amount <= 0)
                return Result.Error("Withdrawal amount must be greater than zero.");

            // Check minimum withdrawal amount
            var minWithdrawalAmount = 0.0001m; // Example: 0.0001 BTC
            if (request.Amount < minWithdrawalAmount)
                return Result.Error($"Minimum withdrawal amount is {minWithdrawalAmount}.");

            // Check if wallet exists and is active
            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(request.SourceWalletAddress, cancellationToken);
            if (wallet == null)
                return Result.NotFound($"Wallet with address '{request.SourceWalletAddress}' not found.");

            if (!wallet.IsActive)
                return Result.Error("The wallet is not active for withdrawals.");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating withdrawal request for wallet {WalletAddress}", 
                request?.SourceWalletAddress ?? "unknown");
            return Result.Error("An error occurred while validating the withdrawal request.");
        }
    }

    private Task<Result> ValidateTransferRequestAsync(TransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null)
                return Task.FromResult(Result.Error("Transfer request cannot be null."));

            // Validate wallet addresses
            if (string.IsNullOrWhiteSpace(request.SourceWalletAddress) || !IsValidWalletAddress(request.SourceWalletAddress))
                return Task.FromResult(Result.Error("Invalid source wallet address format."));

            if (string.IsNullOrWhiteSpace(request.DestinationWalletAddress) || !IsValidWalletAddress(request.DestinationWalletAddress))
                return Task.FromResult(Result.Error("Invalid destination wallet address format."));

            if (string.Equals(request.SourceWalletAddress, request.DestinationWalletAddress, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(Result.Error("Source and destination wallets cannot be the same."));

            // Validate amount
            if (request.Amount <= 0)
                return Task.FromResult(Result.Error("Transfer amount must be greater than zero."));

            // Check minimum transfer amount
            var minTransferAmount = 0.0001m; // Example: 0.0001 BTC
            if (request.Amount < minTransferAmount)
                return Task.FromResult(Result.Error($"Minimum transfer amount is {minTransferAmount} {GetCurrencyCodeFromWalletAddress(request.SourceWalletAddress) ?? "crypto"}."));

            // Validate fee
            if (request.Fee < 0)
                return Task.FromResult(Result.Error("Fee cannot be negative."));
                
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating transfer request from {SourceWallet} to {DestinationWallet}", 
                request?.SourceWalletAddress, request?.DestinationWalletAddress);
            return Task.FromResult(Result.Error("An error occurred while validating the transfer request."));
        }

    }

    private bool IsValidWalletAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;
            
        // Use the WalletAddressValidator for consistent validation
        return address.IsValidWalletAddress();
    }

    private bool IsValidTransactionHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return false;

        // Basic format check for transaction hashes (adjust based on your needs)
        return System.Text.RegularExpressions.Regex.IsMatch(hash, "^[a-fA-F0-9]{64,128}$");
    }

    private bool IsValidIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }
    
    private async Task<Result> ValidateDepositRequestAsync(DepositRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null)
                return Result.Error("Deposit request cannot be null.");

            // Validate wallet address
            if (string.IsNullOrWhiteSpace(request.WalletAddress) || !IsValidWalletAddress(request.WalletAddress))
                return Result.Error("Invalid wallet address format.");

            // Validate amount
            if (request.Amount <= 0)
                return Result.Error("Deposit amount must be greater than zero.");

            // Validate fee (if provided)
            if (request.Fee < 0)
                return Result.Error("Fee cannot be negative.");

            // Check if wallet exists
            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(request.WalletAddress, cancellationToken);
            if (wallet == null)
                return Result.NotFound($"Wallet with address '{request.WalletAddress}' not found.");

            if (!wallet.IsActive)
                return Result.Error("The wallet is not active for deposits.");

            // Validate transaction hash if provided
            if (!string.IsNullOrWhiteSpace(request.TransactionHash) && !IsValidTransactionHash(request.TransactionHash))
                return Result.Error("Invalid transaction hash format.");

            // Validate IP address if provided
            if (!string.IsNullOrWhiteSpace(request.IpAddress) && !IsValidIpAddress(request.IpAddress))
                return Result.Error("Invalid IP address format.");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating deposit request for wallet {WalletAddress}", 
                request?.WalletAddress ?? "unknown");
            return Result.Error("An error occurred while validating the deposit request.");
        }
    }

    private string? GetCurrencyCodeFromWalletAddress(string walletAddress)
    {
        // This is a simplified example - in a real application, you would determine
        // the currency based on the wallet address format or by querying the database
        if (walletAddress.StartsWith("1") || walletAddress.StartsWith("3") || walletAddress.StartsWith("bc1"))
            return "BTC";
        if (walletAddress.StartsWith("0x") && walletAddress.Length == 42)
            return "ETH";
        
        return null;
    }
    
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<WalletDto>>> GetUserWalletsByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching all wallets for user ID: {UserId}", userId);

            var wallets = await _walletRepository.GetUserWalletsByIdWithDetailsAsync(userId, cancellationToken);
            var result = _mapper.Map<IReadOnlyList<WalletDto>>(wallets);

            return Result.Success<IReadOnlyList<WalletDto>>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching wallets for user ID: {UserId}", userId);
            return Result.Error("An error occurred while fetching wallets.");
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<WalletDto>> GetUserWalletByCurrencyAsync(
        Guid userId,
        string currencyCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching {Currency} wallet for user ID: {UserId}", currencyCode, userId);

            if (string.IsNullOrWhiteSpace(currencyCode))
                return Result.Error("Currency code cannot be empty.");
                
            var wallet = await _walletRepository.GetUserWalletByCurrencyWithDetailsAsync(userId, currencyCode, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("{Currency} wallet not found for user ID: {UserId}", currencyCode, userId);
                return Result.NotFound($"{currencyCode} wallet not found for user.");
            }

            var result = _mapper.Map<WalletDto>(wallet);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching {Currency} wallet for user ID: {UserId}", currencyCode, userId);
            return Result.Error($"An error occurred while fetching {currencyCode} wallet.");
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<Dictionary<string, decimal>>> GetUserBalancesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching balances for user ID: {UserId}", userId);

            var wallets = await _walletRepository.GetUserWalletsByIdWithDetailsAsync(userId, cancellationToken);
            var balances = wallets.ToDictionary(
                w => w.Cryptocurrency.Code, 
                w => w.Balance);

            return Result.Success(balances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching balances for user ID: {UserId}", userId);
            return Result.Error("An error occurred while fetching wallet balances.");
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<TransactionDto>> DepositToUserWalletAsync(
        Guid userId,
        string currencyCode,
        decimal amount,
        string? transactionHash = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing deposit of {Amount} {Currency} to user ID: {UserId}", 
                amount, currencyCode, userId);

            // Validate input
            if (amount <= 0)
                return Result.Error("Deposit amount must be greater than zero.");
                
            // Get the user's wallet for the specified currency
            var walletResult = await GetUserWalletByCurrencyAsync(userId, currencyCode, cancellationToken);
            if (!walletResult.IsSuccess)
                return Result.NotFound($"{currencyCode} wallet not found for user.");
                
            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(walletResult.Value?.Address ?? string.Empty, cancellationToken);
            
            if (wallet == null || string.IsNullOrEmpty(wallet.Address))
            {
                _logger.LogError("Wallet not found for user ID: {UserId} and currency: {Currency}", userId, currencyCode);
                return Result.NotFound("Wallet not found or invalid wallet address.");
            }
            
            // Process the deposit
            var depositRequest = new DepositRequest
            {
                WalletAddress = wallet.Address,
                Amount = amount,
                TransactionHash = transactionHash ?? string.Empty
            };
            
            return await DepositFundsAsync(depositRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deposit of {Amount} {Currency} to user ID: {UserId}", 
                amount, currencyCode, userId);
            return Result.Error($"An error occurred while processing the deposit: {ex.Message}");
        }
    }
    
    /// <inheritdoc />
    public async Task<Result<TransactionDto>> WithdrawFromUserWalletAsync(
        Guid userId,
        string currencyCode,
        decimal amount,
        string destinationAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing withdrawal of {Amount} {Currency} from user ID: {UserId} to {Destination}", 
                amount, currencyCode, userId, destinationAddress);

            // Validate input
            if (amount <= 0)
                return Result.Error("Withdrawal amount must be greater than zero.");
                
            if (string.IsNullOrWhiteSpace(destinationAddress))
                return Result.Error("Destination address is required.");
                
            // Get the user's wallet for the specified currency
            var walletResult = await GetUserWalletByCurrencyAsync(userId, currencyCode, cancellationToken);
            if (!walletResult.IsSuccess)
                return Result.NotFound($"{currencyCode} wallet not found for user.");
                
            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(walletResult.Value?.Address ?? string.Empty, cancellationToken);
            
            if (wallet == null || string.IsNullOrEmpty(wallet.Address))
            {
                _logger.LogError("Wallet not found for user ID: {UserId} and currency: {Currency}", userId, currencyCode);
                return Result.NotFound("Wallet not found or invalid wallet address.");
            }
            
            // Process the withdrawal
            var withdrawRequest = new WithdrawRequest
            {
                SourceWalletAddress = wallet.Address,
                Amount = amount,
                DestinationAddress = destinationAddress ?? string.Empty
            };
            
            return await WithdrawFundsAsync(withdrawRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing withdrawal of {Amount} {Currency} from user ID: {UserId}", 
                amount, currencyCode, userId);
            return Result.Error($"An error occurred while processing the withdrawal: {ex.Message}");
        }
    }
}
