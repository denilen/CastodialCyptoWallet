using CryptoWallet.Application.Common.Services;
using CryptoWallet.Application.Wallets.Dtos;
using CryptoWallet.Domain.Transactions;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;

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
            _logger.LogInformation("Processing deposit request for wallet: {WalletAddress}", request.WalletAddress);

            // Validate request
            var validationResult = await ValidateDepositRequestAsync(request, cancellationToken);
            if (!validationResult.IsSuccess)
                return validationResult;

            var wallet = await _walletRepository.GetByAddressWithDetailsAsync(request.WalletAddress, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet with address {WalletAddress} not found", request.WalletAddress);
                return Result.NotFound($"Wallet with address '{request.WalletAddress}' not found.");
            }

            // Update wallet balance
            wallet.Deposit(request.Amount);
            await _walletRepository.UpdateAsync(wallet, cancellationToken);

            // Create transaction record
            var transaction = new Transaction(
                wallet: wallet,
                type: TransactionType.Deposit,
                amount: request.Amount,
                status: TransactionStatus.Completed,
                transactionHash: request.TransactionHash,
                notes: request.Notes,
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully processed deposit of {Amount} {Currency} to wallet {WalletAddress}",
                request.Amount, wallet.Cryptocurrency.Code, request.WalletAddress);

            var result = _mapper.Map<TransactionDto>(transaction);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing deposit to wallet: {WalletAddress}",
                request.WalletAddress);
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
            _logger.LogInformation("Processing withdrawal request from wallet: {WalletAddress}",
                request.SourceWalletAddress);

            // Validate request
            var validationResult = await ValidateWithdrawalRequestAsync(request, cancellationToken);
            if (!validationResult.IsSuccess)
                return validationResult;

            var wallet =
                await _walletRepository.GetByAddressWithDetailsAsync(request.SourceWalletAddress, cancellationToken);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet with address {WalletAddress} not found", request.SourceWalletAddress);
                return Result.NotFound($"Wallet with address '{request.SourceWalletAddress}' not found.");
            }

            // Check if wallet has sufficient balance (including pending withdrawals)
            var balanceResult = await GetWalletBalanceAsync(request.SourceWalletAddress, cancellationToken);
            if (!balanceResult.IsSuccess)
                return Result.Error(balanceResult.Errors.First());

            var availableBalance = balanceResult.Value.AvailableBalance;
            if (availableBalance < request.Amount + (request.Fee ?? 0))
            {
                _logger.LogWarning(
                    "Insufficient balance in wallet {WalletAddress}. Available: {Available}, Requested: {Requested}",
                    request.SourceWalletAddress, availableBalance, request.Amount + (request.Fee ?? 0));
                return Result.Error("Insufficient balance to complete the withdrawal.");
            }

            // Update wallet balance
            wallet.Withdraw(request.Amount + (request.Fee ?? 0));
            await _walletRepository.UpdateAsync(wallet, cancellationToken);

            // Create withdrawal transaction
            var transaction = new Transaction(
                wallet: wallet,
                type: TransactionType.Withdrawal,
                amount: request.Amount,
                fee: request.Fee,
                status: TransactionStatus.Pending, // Will be updated when confirmed on the blockchain
                destinationAddress: request.DestinationAddress,
                notes: request.Notes,
                ipAddress: request.IpAddress,
                userAgent: request.UserAgent);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            await _transactionRepository.SaveChangesAsync(cancellationToken);

            // TODO: Initiate blockchain withdrawal (would be handled by a background service)
            // _blockchainService.InitiateWithdrawal(transaction);

            _logger.LogInformation(
                "Successfully processed withdrawal of {Amount} {Currency} from wallet {WalletAddress}",
                request.Amount, wallet.Cryptocurrency.Code, request.SourceWalletAddress);

            var result = _mapper.Map<TransactionDto>(transaction);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing withdrawal from wallet: {WalletAddress}",
                request.SourceWalletAddress);
            return Result.Error(
                $"An error occurred while processing the withdrawal from wallet '{request.SourceWalletAddress}'.");
        }
    }

    /// <inheritdoc />
    public async Task<Result<TransactionDto>> TransferFundsAsync(
        TransferRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing transfer from {SourceWallet} to {DestinationWallet}",
                request.SourceWalletAddress, request.DestinationWalletAddress);

            // Validate request
            var validationResult = await ValidateTransferRequestAsync(request, cancellationToken);
            if (!validationResult.IsSuccess)
                return validationResult;

            // Get source and destination wallets
            var sourceWallet =
                await _walletRepository.GetByAddressWithDetailsAsync(request.SourceWalletAddress, cancellationToken);
            var destinationWallet =
                await _walletRepository.GetByAddressWithDetailsAsync(request.DestinationWalletAddress,
                    cancellationToken);

            if (sourceWallet == null || destinationWallet == null)
            {
                var error = sourceWallet == null
                    ? $"Source wallet with address '{request.SourceWalletAddress}' not found."
                    : $"Destination wallet with address '{request.DestinationWalletAddress}' not found.";

                _logger.LogWarning(error);
                return Result.NotFound(error);
            }

            // Check if wallets are for the same cryptocurrency
            if (sourceWallet.CryptocurrencyId != destinationWallet.CryptocurrencyId)
            {
                const string error = "Source and destination wallets must be for the same cryptocurrency.";
                _logger.LogWarning(error);
                return Result.Error(error);
            }

            // Check if source wallet has sufficient balance
            var balanceResult = await GetWalletBalanceAsync(request.SourceWalletAddress, cancellationToken);
            if (!balanceResult.IsSuccess)
                return Result.Error(balanceResult.Errors.First());

            var availableBalance = balanceResult.Value.AvailableBalance;
            if (availableBalance < request.Amount + (request.Fee ?? 0))
            {
                _logger.LogWarning(
                    "Insufficient balance in source wallet {WalletAddress}. Available: {Available}, Requested: {Requested}",
                    request.SourceWalletAddress, availableBalance, request.Amount + (request.Fee ?? 0));
                return Result.Error("Insufficient balance to complete the transfer.");
            }

            // Perform the transfer within a transaction
            using var transaction = await _walletRepository.BeginTransactionAsync(cancellationToken);

            try
            {
                // Update source wallet (deduct amount + fee)
                sourceWallet.Withdraw(request.Amount + (request.Fee ?? 0));
                await _walletRepository.UpdateAsync(sourceWallet, cancellationToken);

                // Update destination wallet (add amount)
                destinationWallet.Deposit(request.Amount);
                await _walletRepository.UpdateAsync(destinationWallet, cancellationToken);

                // Create transfer transaction (from source to destination)
                var transferTransaction = new Transaction(
                    wallet: sourceWallet,
                    type: TransactionType.TransferOut,
                    amount: request.Amount,
                    fee: request.Fee,
                    status: TransactionStatus.Completed,
                    destinationAddress: destinationWallet.Address,
                    notes: request.Notes,
                    ipAddress: request.IpAddress,
                    userAgent: request.UserAgent);

                // Create corresponding receive transaction for the destination wallet
                var receiveTransaction = new Transaction(
                    wallet: destinationWallet,
                    type: TransactionType.TransferIn,
                    amount: request.Amount,
                    status: TransactionStatus.Completed,
                    relatedTransactionId: transferTransaction.Id,
                    notes: request.Notes,
                    ipAddress: request.IpAddress,
                    userAgent: request.UserAgent);

                // Set the related transaction ID after receive transaction is created
                transferTransaction.RelatedTransactionId = receiveTransaction.Id;

                // Save transactions
                await _transactionRepository.AddAsync(transferTransaction, cancellationToken);
                await _transactionRepository.AddAsync(receiveTransaction, cancellationToken);
                await _transactionRepository.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully transferred {Amount} {Currency} from {SourceWallet} to {DestinationWallet}",
                    request.Amount, sourceWallet.Cryptocurrency.Code, request.SourceWalletAddress,
                    request.DestinationWalletAddress);

                var result = _mapper.Map<TransactionDto>(transferTransaction);
                return Result.Success(result);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while transferring funds from {SourceWallet} to {DestinationWallet}",
                request.SourceWalletAddress, request.DestinationWalletAddress);
            return Result.Error($"An error occurred while processing the transfer: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private async Task<Result> ValidateDepositRequestAsync(DepositRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return Result.Error("Deposit request cannot be null.");

        if (request.Amount <= 0)
            return Result.Error("Deposit amount must be greater than zero.");

        // Add any additional validation rules here

        return Result.Success();
    }

    private async Task<Result> ValidateWithdrawalRequestAsync(WithdrawRequest request,
                                                              CancellationToken cancellationToken)
    {
        if (request == null)
            return Result.Error("Withdrawal request cannot be null.");

        if (request.Amount <= 0)
            return Result.Error("Withdrawal amount must be greater than zero.");

        if (request.Fee < 0)
            return Result.Error("Fee cannot be negative.");

        // Add any additional validation rules here

        return Result.Success();
    }

    private async Task<Result> ValidateTransferRequestAsync(TransferRequest request,
                                                            CancellationToken cancellationToken)
    {
        if (request == null)
            return Result.Error("Transfer request cannot be null.");

        if (string.Equals(request.SourceWalletAddress, request.DestinationWalletAddress,
                StringComparison.OrdinalIgnoreCase))
            return Result.Error("Source and destination wallets cannot be the same.");

        if (request.Amount <= 0)
            return Result.Error("Transfer amount must be greater than zero.");

        if (request.Fee < 0)
            return Result.Error("Fee cannot be negative.");

        // Add any additional validation rules here

        return Result.Success();
    }

    #endregion
}
