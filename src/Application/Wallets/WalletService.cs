using Ardalis.Result;
using AutoMapper;
using CryptoWallet.Application.Common.Interfaces;
using CryptoWallet.Application.Common.Services;
using CryptoWallet.Application.Wallets.Dtos;
using CryptoWallet.Domain.Currencies;
using CryptoWallet.Domain.Enums;
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
                // Create deposit transaction
                var depositTransaction = new Transaction(
                    wallet,
                    TransactionTypeEnum.Deposit,
                    request.Amount,
                    request.Fee ?? 0,
                    wallet.Cryptocurrency.Code,
                    request.SourceAddress,
                    request.WalletAddress,
                    request.Notes ?? $"Deposit from {request.SourceAddress}");

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

                // Process deposit in a transaction scope
                using var dbContextTransaction = await _transactionRepository.BeginTransactionAsync();
                try
                {
                    // Add transaction to repository and update wallet
                    await _walletRepository.UpdateAsync(wallet);
                    await _transactionRepository.AddAsync(depositTransaction);
                    
                    // Commit the transaction
                    await dbContextTransaction.CommitAsync();

                    // Mark transaction as completed
                    depositTransaction.MarkAsCompleted();
                    await _transactionRepository.SaveChangesAsync();

                    _logger.LogInformation(
                        "Successfully deposited {Amount} {Currency} to wallet {WalletAddress}",
                        request.Amount, 
                        wallet.Cryptocurrency.Code,
                        request.WalletAddress);

                    return Result.Success(_mapper.Map<TransactionDto>(depositTransaction));
                }
                catch (Exception ex)
                {
                    await dbContextTransaction.RollbackAsync();
                    _logger.LogError(ex, "Failed to process deposit to wallet {WalletAddress}", request.WalletAddress);
                    return Result.Error($"Failed to process deposit: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while depositing to wallet {WalletAddress}", request.WalletAddress);
                return Result.Error($"An error occurred while processing the deposit: {ex.Message}");
            }

            return Result.Success(_mapper.Map<TransactionDto>(depositTransaction));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while depositing to wallet {WalletAddress}", request.WalletAddress);
            return Result.Error($"An error occurred while processing the deposit to wallet '{request.WalletAddress}'.");
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
            var balanceResult = await GetWalletBalanceAsync(request.WalletAddress, cancellationToken);
            if (!balanceResult.IsSuccess)
                return balanceResult;

            var availableBalance = balanceResult.Value.AvailableBalance;
            
            // Check daily withdrawal limit if applicable
            if (wallet != null)
            {
                var maxDailyWithdrawal = 10000m; // Example: 10,000 USD equivalent
                var dailyWithdrawals = await _transactionRepository.GetDailyWithdrawalAmountAsync(
                    wallet.Id, 
                    DateTimeOffset.UtcNow.AddDays(-1), 
                    cancellationToken);

                if (dailyWithdrawals + request.Amount > maxDailyWithdrawal)
                    return Result.Error($"Daily withdrawal limit of {maxDailyWithdrawal} {wallet.Cryptocurrency.Code} exceeded.");
            }

            // Validate fee
            if (request.Fee < 0)
                return Result.Error("Fee cannot be negative.");

            if (availableBalance < request.Amount + (request.Fee ?? 0))
                return Result.Error("Insufficient balance to complete the withdrawal.");

            // Validate IP address and user agent for security
            if (!string.IsNullOrWhiteSpace(request.IpAddress) && !IsValidIpAddress(request.IpAddress))
                return Result.Error("Invalid IP address format.");

            if (string.IsNullOrWhiteSpace(request.UserAgent))
                return Result.Error("User-Agent header is required for security reasons.");

            // Create withdrawal transaction
            var withdrawalTransaction = new Transaction(
                wallet,
                TransactionTypeEnum.Withdrawal,
                request.Amount,
                request.Fee ?? 0,
                wallet.Cryptocurrency.Code,
                request.SourceWalletAddress,
                request.DestinationAddress,
                request.Notes);

            // Mark transaction as processing
            withdrawalTransaction.MarkAsProcessing();

            // Process withdrawal in a transaction scope
            using var dbContextTransaction = await _transactionRepository.BeginTransactionAsync();
            try
            {
                // Withdraw from wallet (includes fee)
                wallet.Withdraw(request.Amount + (request.Fee ?? 0));
                
                // Update wallet and add transaction
                await _walletRepository.UpdateAsync(wallet);
                await _transactionRepository.AddAsync(withdrawalTransaction);
                
                // Commit the transaction
                await dbContextTransaction.CommitAsync();

                // Mark transaction as completed
                withdrawalTransaction.MarkAsCompleted();
                await _transactionRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully withdrew {Amount} {Currency} from wallet {WalletAddress} to {DestinationAddress}",
                    request.Amount, 
                    wallet.Cryptocurrency.Code,
                    request.WalletAddress,
                    request.DestinationAddress);

                return Result.Success(_mapper.Map<TransactionDto>(withdrawalTransaction));
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to process withdrawal from wallet {WalletAddress}", request.SourceWalletAddress);
                return Result.Error("Failed to process withdrawal: " + ex.Message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while processing withdrawal from wallet {WalletAddress}", request.SourceWalletAddress);
                return Result.Error("An unexpected error occurred while processing the withdrawal.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing withdrawal from wallet {WalletAddress}", request.SourceWalletAddress);
            return Result.Error($"An error occurred while processing the withdrawal from wallet '{request.SourceWalletAddress}'.");
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
                return balanceResult;

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

            // Process transfer in a transaction scope
            using var dbContextTransaction = await _transactionRepository.BeginTransactionAsync();
            try
            {
                // Withdraw from source wallet (includes fee)
                sourceWallet.Withdraw(request.Amount + (request.Fee ?? 0));
                
                // Deposit to destination wallet (amount only, no fee)
                destinationWallet.Deposit(request.Amount);
                
                // Update wallets and add transactions
                await _walletRepository.UpdateAsync(sourceWallet);
                await _walletRepository.UpdateAsync(destinationWallet);
                
                await _transactionRepository.AddAsync(transferOutTransaction);
                await _transactionRepository.AddAsync(transferInTransaction);
                
                // Commit the transaction
                await dbContextTransaction.CommitAsync();

                // Mark transactions as completed
                transferOutTransaction.MarkAsCompleted();
                transferInTransaction.MarkAsCompleted();
                
                await _transactionRepository.SaveChangesAsync();

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
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to process transfer from {SourceWallet} to {DestinationWallet}",
                    request.SourceWalletAddress, request.DestinationWalletAddress);
                return Result.Error("Failed to process transfer: " + ex.Message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error occurred while processing transfer from {SourceWallet} to {DestinationWallet}",
                    request.SourceWalletAddress, request.DestinationWalletAddress);
                return Result.Error("An unexpected error occurred while processing the transfer.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in TransferFundsAsync");
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

    private async Task<Result> ValidateTransferRequestAsync(TransferRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null)
                return Result.Error("Transfer request cannot be null.");

            // Validate wallet addresses
            if (string.IsNullOrWhiteSpace(request.SourceWalletAddress) || !IsValidWalletAddress(request.SourceWalletAddress))
                return Result.Error("Invalid source wallet address format.");

            if (string.IsNullOrWhiteSpace(request.DestinationWalletAddress) || !IsValidWalletAddress(request.DestinationWalletAddress))
                return Result.Error("Invalid destination wallet address format.");

            if (string.Equals(request.SourceWalletAddress, request.DestinationWalletAddress, StringComparison.OrdinalIgnoreCase))
                return Result.Error("Source and destination wallets cannot be the same.");

            // Validate amount
            if (request.Amount <= 0)
                return Result.Error("Transfer amount must be greater than zero.");

            // Check minimum transfer amount
            var minTransferAmount = 0.0001m; // Example: 0.0001 BTC
            if (request.Amount < minTransferAmount)
                return Result.Error($"Minimum transfer amount is {minTransferAmount} {GetCurrencyCodeFromWalletAddress(request.SourceWalletAddress) ?? "crypto"}.");

            // Validate fee
            if (request.Fee < 0)
                return Result.Error("Fee cannot be negative.");
                
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating transfer request from {SourceWallet} to {DestinationWallet}", 
                request?.SourceWalletAddress, request?.DestinationWalletAddress);
            return Result.Error("An error occurred while validating the transfer request.");
        }

    }

    private bool IsValidWalletAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        // Basic length check (adjust based on your cryptocurrency requirements)
        if (address.Length < 26 || address.Length > 64)
            return false;

        // This is a simplified example - you should implement specific validation for each cryptocurrency
        return System.Text.RegularExpressions.Regex.IsMatch(address, "^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$|^0x[a-fA-F0-9]{40}$");
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
}
