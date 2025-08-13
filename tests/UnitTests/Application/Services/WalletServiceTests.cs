using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using AutoMapper;
using CryptoWallet.Application.Wallets;
using CryptoWallet.Domain.Currencies;
using CryptoWallet.Domain.Enums;
using CryptoWallet.Domain.Interfaces.Repositories;
using CryptoWallet.Domain.Models.DTOs.Wallets;
using CryptoWallet.Domain.Transactions;
using CryptoWallet.Domain.Users;
using CryptoWallet.Domain.Wallets;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace CryptoWallet.UnitTests.Application.Services;

public class WalletServiceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Mock<ILogger<WalletService>> _loggerMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICryptocurrencyRepository> _cryptocurrencyRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly WalletService _walletService;
    private readonly Fixture _fixture;

    public WalletServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _loggerMock = new Mock<ILogger<WalletService>>();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _cryptocurrencyRepositoryMock = new Mock<ICryptocurrencyRepository>();
        _mapperMock = new Mock<IMapper>();
        _fixture = new Fixture();

        // Setup default mapper behavior for common types
        _mapperMock.Setup(m => m.Map<WalletDto>(It.IsAny<Wallet>()))
            .Returns((Wallet source) => new WalletDto
            {
                Id = source.Id,
                UserId = source.UserId,
                CurrencyCode = source.Cryptocurrency.Code,
                Balance = source.Balance,
                Address = source.Address,
                IsActive = source.IsActive,
                CreatedAt = source.CreatedAt
            });

        // Setup default mapper for Transaction to TransactionDto
        _mapperMock.Setup(m => m.Map<TransactionDto>(It.IsAny<Transaction>()))
            .Returns((Transaction source) => new TransactionDto
            {
                Id = source.Id,
                WalletAddress = source.Wallet?.Address ?? string.Empty,
                TransactionType = source.TypeEnum.ToString(),
                Status = source.StatusEnum.ToString(),
                Amount = source.Amount,
                Fee = source.Fee,
                CurrencyCode = source.Wallet?.Cryptocurrency?.Code ?? string.Empty,
                TransactionHash = source.TransactionHash,
                Notes = source.Description,
                CreatedAt = source.CreatedAt,
                UpdatedAt = source.LastModifiedAt,
                IpAddress = null,
                UserAgent = null,
                Metadata = source.Metadata
            });

        _walletService = new WalletService(
            _loggerMock.Object,
            _walletRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            _cryptocurrencyRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetWalletByAddressAsync_WhenWalletExists_ShouldReturnWallet()
    {
        // Arrange
        var walletAddress = "0x1234567890abcdef1234567890abcdef12345678";
        var user = new User("test@example.com", "hashedpassword");
        var crypto = new Cryptocurrency("BTC", "Bitcoin");
        var wallet = new Wallet(user, crypto, walletAddress);
        wallet.Deposit(100m, "Initial deposit");

        var walletDto = new WalletDto
        {
            Id = wallet.Id,
            UserId = wallet.UserId,
            CurrencyCode = wallet.Cryptocurrency.Code,
            CurrencyName = wallet.Cryptocurrency.Name,
            Balance = wallet.Balance,
            Address = wallet.Address,
            IsDefault = false, // Not supported in Wallet class
            IsActive = wallet.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(walletAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _mapperMock
            .Setup(x => x.Map<WalletDto>(wallet))
            .Returns(walletDto);

        // Act
        var result = await _walletService.GetWalletByAddressAsync(walletAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(walletDto);
    }

    [Fact]
    public async Task GetWalletByAddressAsync_WhenWalletDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var walletAddress = "nonexistent-address";

        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(walletAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Wallet));

        // Act
        var result = await _walletService.GetWalletByAddressAsync(walletAddress);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task GetWalletBalanceAsync_WithValidRequest_ShouldReturnBalance()
    {
        // Arrange
        var user = new User("test@example.com", "hashedpassword");
        var crypto = new Cryptocurrency("BTC", "Bitcoin");
        var wallet = new Wallet(user, crypto, "0x1234567890");
        wallet.Deposit(100m, "Initial deposit");

        // Create a pending withdrawal transaction
        var pendingWithdrawal = new Transaction(
            wallet,
            TransactionTypeEnum.Withdrawal,
            20m,                // amount
            0.001m,             // fee
            "BTC",              // feeCurrency
            wallet.Address,     // fromAddress
            "0xrecipient",      // toAddress
            "Test withdrawal"); // description

        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(wallet.Address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _transactionRepositoryMock
            .Setup(x => x.GetPendingWithdrawalsAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Transaction> { pendingWithdrawal });

        // Act
        var result = await _walletService.GetWalletBalanceAsync(wallet.Address);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AvailableBalance.Should().Be(80m); // 100 - 20 (pending withdrawal)
        result.Value.LockedBalance.Should().Be(20m);    // pending withdrawal amount
        result.Value.TotalBalance.Should().Be(100m);    // total balance remains the same
        result.Value.CurrencyCode.Should().Be("BTC");
        result.Value.Address.Should().Be(wallet.Address);
    }

    #region WithdrawFundsAsync Tests

    private (User user, Cryptocurrency crypto, Wallet wallet) SetupWithdrawalTestData(decimal initialBalance = 100m)
    {
        var walletAddress = "0x1234567890abcdef1234567890abcdef12345678";

        // Create user with ID
        var user = new User("test@example.com", "hashedpassword");
        user.GetType().GetProperty("Id")?.SetValue(user, Guid.NewGuid());

        // Create cryptocurrency with ID
        var crypto = new Cryptocurrency("BTC", "Bitcoin");
        var cryptoId = Guid.NewGuid();
        crypto.GetType().GetProperty("Id")?.SetValue(crypto, cryptoId);

        // Create wallet with proper initialization
        var wallet = new Wallet(user, crypto, walletAddress);
        var walletId = Guid.NewGuid();
        wallet.GetType().GetProperty("Id")?.SetValue(wallet, walletId);

        // Deposit initial funds
        wallet.Deposit(initialBalance, "Test deposit");

        return (user, crypto, wallet);
    }

    private void SetupWithdrawalMocks(
        User user,
        Cryptocurrency crypto,
        Wallet wallet,
        decimal dailyWithdrawn = 0m,
        List<Transaction>? pendingWithdrawals = null)
    {
        // Setup wallet repository
        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(wallet.Address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _walletRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Setup transaction repository
        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transactionRepositoryMock
            .Setup(x => x.GetTotalWithdrawnAmountAsync(
                user.Id,
                crypto.Id,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyWithdrawn);

        _transactionRepositoryMock
            .Setup(x => x.GetPendingWithdrawalsAsync(wallet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingWithdrawals ?? new List<Transaction>());

        _transactionRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Setup cryptocurrency repository
        _cryptocurrencyRepositoryMock
            .Setup(x => x.GetByCodeAsync(crypto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(crypto);
    }

    private (User user, Cryptocurrency crypto, Wallet wallet) SetupDepositTestData()
    {
        var user = new User("deposit@example.com", "hashedpassword");
        var crypto = new Cryptocurrency("ETH", "Ethereum");
        var wallet = new Wallet(user, crypto, "0x742d35Cc6634C0532925a3b844Bc454e4438f44e");
        return (user, crypto, wallet);
    }

    private void SetupDepositMocks(
        User user,
        Cryptocurrency crypto,
        Wallet wallet,
        string transactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef")
    {
        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(wallet.Address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _cryptocurrencyRepositoryMock
            .Setup(x => x.GetByCodeAsync(crypto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(crypto);

        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns((Transaction t, CancellationToken _) =>
            {
                t.GetType().GetProperty("Id")?.SetValue(t, Guid.NewGuid());
                return Task.FromResult(t);
            });

        _walletRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(m => m.Map<TransactionDto>(It.IsAny<Transaction>()))
            .Returns((Transaction t) => new TransactionDto
            {
                Id = (Guid)(t.GetType().GetProperty("Id")?.GetValue(t) ?? Guid.Empty),
                WalletAddress = t.Wallet?.Address ?? string.Empty,
                TransactionType = t.TypeEnum.ToString(),
                Status = t.StatusEnum.ToString(),
                Amount = t.Amount,
                Fee = t.Fee,
                CurrencyCode = t.Wallet?.Cryptocurrency?.Code ?? string.Empty,
                TransactionHash = t.TransactionHash,
                Notes = t.Description,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.LastModifiedAt
            });
    }

    #region Deposit Tests

    [Fact]
    public async Task DepositFundsAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var (user, crypto, wallet) = SetupDepositTestData();
        var amount = 10m;
        var transactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef";

        SetupDepositMocks(user, crypto, wallet, transactionHash);

        var depositRequest = new DepositRequest
        {
            WalletAddress = wallet.Address,
            Amount = amount,
            TransactionHash = transactionHash,
            Notes = "Test deposit",
            IpAddress = "192.168.1.1",
            UserAgent = "test-agent"
        };

        // Act
        var result = await _walletService.DepositFundsAsync(depositRequest);

        // Assert
        result.IsSuccess.Should().BeTrue("Deposit should succeed with valid request");
        wallet.Balance.Should().Be(amount, "Wallet balance should be increased by the deposit amount");

        _transactionRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Transaction>(t =>
                    t.Amount == amount &&
                    t.TypeEnum == TransactionTypeEnum.Deposit &&
                    t.StatusEnum == TransactionStatusEnum.Completed &&
                    t.TransactionHash == transactionHash),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "Should create a completed deposit transaction");

        _walletRepositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "Should save changes to wallet balance");
    }

    [Fact]
    public async Task DepositFundsAsync_WithInvalidWalletAddress_ShouldFail()
    {
        // Arrange
        var depositRequest = new DepositRequest
        {
            WalletAddress = "invalid-address",
            Amount = 10m,
            TransactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            IpAddress = "192.168.1.1",
            UserAgent = "test-agent"
        };

        // Act
        var result = await _walletService.DepositFundsAsync(depositRequest);

        // Assert
        result.IsSuccess.Should().BeFalse("Deposit should fail with invalid wallet address");
        result.Errors.Should().Contain("Invalid wallet address format.");

        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Should not create transaction for invalid wallet address");
    }

    [Fact]
    public async Task DepositFundsAsync_WithDuplicateTransactionHash_ShouldFail()
    {
        // Arrange
        var (user, crypto, wallet) = SetupDepositTestData();
        var amount = 10m;
        // Use a properly formatted transaction hash (64 hex characters without 0x prefix)
        var transactionHash = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcd";

        SetupDepositMocks(user, crypto, wallet, transactionHash);

        // Simulate existing transaction with same hash (the service will add the 0x prefix)
        var existingTransaction = new Transaction(
            wallet,
            TransactionTypeEnum.Deposit,
            amount,
            0.001m,
            crypto.Code,
            null,
            null,
            "Test deposit");
            
        _transactionRepositoryMock
            .Setup(x => x.GetByTransactionHashAsync(transactionHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTransaction);

        var depositRequest = new DepositRequest
        {
            WalletAddress = wallet.Address,
            Amount = amount,
            TransactionHash = transactionHash,
            IpAddress = "192.168.1.1",
            UserAgent = "test-agent"
        };

        // Act
        var result = await _walletService.DepositFundsAsync(depositRequest);

        // Assert
        result.IsSuccess.Should().BeFalse("Deposit should fail with duplicate transaction hash");
        result.Errors.Should().Contain("Transaction with this hash already exists.");

        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Should not create duplicate transaction");
            
        _transactionRepositoryMock.Verify(
            x => x.GetByTransactionHashAsync(transactionHash, It.IsAny<CancellationToken>()),
            Times.Once,
            "Should check for duplicate transaction hash");
    }

    [Fact]
    public async Task DepositFundsAsync_WithNegativeAmount_ShouldFail()
    {
        // Arrange
        var depositRequest = new DepositRequest
        {
            WalletAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e",
            Amount = -10m,
            TransactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            IpAddress = "192.168.1.1",
            UserAgent = "test-agent"
        };

        // Act
        var result = await _walletService.DepositFundsAsync(depositRequest);

        // Assert
        result.IsSuccess.Should().BeFalse("Deposit should fail with negative amount");
        result.Errors.Should().Contain("Deposit amount must be greater than zero.");
    }

    [Fact]
    public async Task DepositFundsAsync_WithInvalidIpAddress_ShouldFail()
    {
        // Arrange
        var depositRequest = new DepositRequest
        {
            WalletAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e",
            Amount = 10m,
            TransactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            IpAddress = "invalid-ip",
            UserAgent = "test-agent"
        };

        // Act
        var result = await _walletService.DepositFundsAsync(depositRequest);

        // Assert
        result.IsSuccess.Should().BeFalse("Deposit should fail with invalid IP address");
        result.Errors.Should().Contain("Invalid IP address format.");
    }

    [Fact]
    public async Task DepositFundsAsync_WithoutUserAgent_ShouldFail()
    {
        // Arrange
        var depositRequest = new DepositRequest
        {
            WalletAddress = "0x742d35Cc6634C0532925a3b844Bc454e4438f44e",
            Amount = 10m,
            TransactionHash = "0x1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef",
            IpAddress = "192.168.1.1",
            UserAgent = string.Empty
        };

        // Act
        var result = await _walletService.DepositFundsAsync(depositRequest);

        // Assert
        result.IsSuccess.Should().BeFalse("Deposit should fail without User-Agent");
        result.Errors.Should().Contain("User-Agent header is required.");
    }

    #endregion

    [Fact]
    public async Task WithdrawFundsAsync_WithSufficientBalance_ShouldSucceed()
    {
        // Arrange
        var (user, crypto, wallet) = SetupWithdrawalTestData();
        var initialBalance = wallet.Balance;
        var amount = 50m;
        var fee = 0.001m;
        var totalWithdrawalAmount = amount + fee;

        var withdrawRequest = new WithdrawRequest
        {
            SourceWalletAddress = wallet.Address,
            Amount = amount,
            Fee = fee,
            // Using a valid Bitcoin testnet address (P2SH format)
            DestinationAddress = "2N4Q5FhU2497BryFfUgbqkAJE87aKHUhXMp",
            Notes = "Test withdrawal",
            IpAddress = "192.168.1.1",
            UserAgent = "test-agent"
        };

        // Setup mocks to capture the transaction
        Transaction? capturedTransaction = null;
        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Callback<Transaction, CancellationToken>((t, _) => capturedTransaction = t)
            .Returns(Task.CompletedTask);

        SetupWithdrawalMocks(user, crypto, wallet);

        // Act
        var result = await _walletService.WithdrawFundsAsync(withdrawRequest);

        // Assert
        if (!result.IsSuccess)
        {
            var errorMessage = string.Join("; ", result.Errors);
            _testOutputHelper.WriteLine($"Withdrawal failed with error: {errorMessage}");
            result.IsSuccess.Should().BeTrue($"Withdrawal should succeed with sufficient balance. Error: {errorMessage}");
        }
        else
        {
            result.IsSuccess.Should().BeTrue("Withdrawal should succeed with sufficient balance");
        }
        
        // Verify wallet balance was updated correctly
        wallet.Balance.Should().Be(initialBalance - totalWithdrawalAmount, 
            $"Balance should be reduced by amount + fee. Expected: {initialBalance - totalWithdrawalAmount}, Actual: {wallet.Balance}");

        // Verify transaction was added with correct parameters
        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Transaction>(t =>
                    t.Amount == amount &&
                    t.Fee == fee &&
                    t.TypeEnum == TransactionTypeEnum.Withdrawal &&
                    t.StatusEnum == TransactionStatusEnum.Completed),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "Transaction should be added with correct parameters");
            
        // Verify the captured transaction
        capturedTransaction.Should().NotBeNull("A transaction should have been created");
        if (capturedTransaction != null)
        {
            capturedTransaction.Amount.Should().Be(amount);
            capturedTransaction.Fee.Should().Be(fee);
            capturedTransaction.TypeEnum.Should().Be(TransactionTypeEnum.Withdrawal);
            capturedTransaction.StatusEnum.Should().Be(TransactionStatusEnum.Completed);
        }
    }

    [Fact]
    public async Task WithdrawFundsAsync_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var (user, crypto, wallet) = SetupWithdrawalTestData(initialBalance: 10m);
        var amount = 20m; // More than the wallet balance
        var fee = 0.001m;

        var withdrawRequest = new WithdrawRequest
        {
            SourceWalletAddress = wallet.Address,
            Amount = amount,
            Fee = fee,
            // Using a valid Bitcoin testnet address (P2SH format)
            DestinationAddress = "2N4Q5FhU2497BryFfUgbqkAJE87aKHUhXMp",
            Notes = "Test withdrawal",
            IpAddress = "192.168.1.1",
            UserAgent = "test-agent"
        };

        SetupWithdrawalMocks(user, crypto, wallet);

        // Act
        var result = await _walletService.WithdrawFundsAsync(withdrawRequest);

        // Assert
        result.IsSuccess.Should().BeFalse("Withdrawal should fail with insufficient balance");
        result.Errors.Should().Contain("Недостаточно средств для выполнения вывода.");
        wallet.Balance.Should().Be(10m, "Balance should remain unchanged");
    }

    [Fact]
    public async Task WithdrawFundsAsync_ExceedingDailyLimit_ShouldFail()
    {
        // Arrange
        var (user, crypto, wallet) = SetupWithdrawalTestData(initialBalance: 10000m);
        var dailyWithdrawn = 9000m; // Already withdrawn today
        var amount = 2000m;         // Would exceed daily limit of 10000m
        var fee = 0.001m;

        var withdrawRequest = new WithdrawRequest
        {
            SourceWalletAddress = wallet.Address,
            Amount = amount,
            Fee = fee,
            // Using a valid Bitcoin testnet address (P2SH format)
            DestinationAddress = "2N4Q5FhU2497BryFfUgbqkAJE87aKHUhXMp",
            Notes = "Test withdrawal",
            IpAddress = "192.168.1.1",
            UserAgent = "test-agent"
        };

        SetupWithdrawalMocks(user, crypto, wallet, dailyWithdrawn);

        // Act
        var result = await _walletService.WithdrawFundsAsync(withdrawRequest);

        // Assert
        result.IsSuccess.Should().BeFalse("Withdrawal should fail when exceeding daily limit");
        result.Errors.Should().Contain("Превышен дневной лимит вывода");
        wallet.Balance.Should().Be(10000m, "Balance should remain unchanged");
    }

    [Fact]
    public async Task WithdrawFundsAsync_WithInvalidIpAddress_ShouldFail()
    {
        // Arrange
        var (_, _, wallet) = SetupWithdrawalTestData();

        var withdrawRequest = new WithdrawRequest
        {
            SourceWalletAddress = wallet.Address,
            Amount = 10m,
            // Using a valid Bitcoin testnet address (P2SH format)
            DestinationAddress = "2N4Q5FhU2497BryFfUgbqkAJE87aKHUhXMp",
            IpAddress = "invalid-ip",
            UserAgent = "test-agent"
        };

        // Act
        var result = await _walletService.WithdrawFundsAsync(withdrawRequest);

        // Assert
        result.IsSuccess.Should().BeFalse("Withdrawal should fail with invalid IP address");
        result.Errors.Should().Contain("Неверный формат IP-адреса.");
    }

    [Fact]
    public async Task WithdrawFundsAsync_WithoutUserAgent_ShouldFail()
    {
        // Arrange
        var (_, _, wallet) = SetupWithdrawalTestData();

        var withdrawRequest = new WithdrawRequest
        {
            SourceWalletAddress = wallet.Address,
            Amount = 10m,
            // Using a valid Bitcoin testnet address (P2SH format)
            DestinationAddress = "2N4Q5FhU2497BryFfUgbqkAJE87aKHUhXMp",
            IpAddress = "192.168.1.1"
            // UserAgent is not set
        };

        // Act
        var result = await _walletService.WithdrawFundsAsync(withdrawRequest);

        // Assert
        result.IsSuccess.Should().BeFalse("Withdrawal should fail without User-Agent");
        result.Errors.Should().Contain("Заголовок User-Agent обязателен");
    }

    #endregion

    #region Transfer Tests

    [Fact]
    public async Task TransferFundsAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var (user, crypto, sourceWallet) = SetupWithdrawalTestData(initialBalance: 100m);
        var (_, _, destinationWallet) = SetupDepositTestData();
        var amount = 10m;
        var fee = 0.1m;

        SetupWithdrawalMocks(user, crypto, sourceWallet);

        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(destinationWallet.Address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationWallet);

        var transferRequest = new TransferRequest
        {
            SourceWalletAddress = sourceWallet.Address,
            DestinationWalletAddress = destinationWallet.Address,
            Amount = amount,
            Fee = fee,
            IpAddress = "192.168.1.1",
            UserAgent = "test-agent"
        };

        // Act
        var result = await _walletService.TransferFundsAsync(transferRequest);

        // Assert
        result.IsSuccess.Should().BeTrue($"Expected success but got error: {string.Join("; ", result.Errors)}");
        sourceWallet.Balance.Should().Be(89.9m, "Source wallet should be debited by amount + fee");
        destinationWallet.Balance.Should().Be(amount, "Destination wallet should be credited by amount");

        // Verify transactions were added
        _transactionRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Transaction>(t =>
                    t.WalletId == sourceWallet.Id &&
                    t.TypeEnum == TransactionTypeEnum.Transfer &&
                    t.StatusEnum == TransactionStatusEnum.Completed &&
                    t.Amount == -amount &&
                    t.Fee == fee),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "Should create a transfer out transaction");

        _transactionRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Transaction>(t =>
                    t.WalletId == destinationWallet.Id &&
                    t.TypeEnum == TransactionTypeEnum.Transfer &&
                    t.StatusEnum == TransactionStatusEnum.Completed &&
                    t.Amount == amount &&
                    t.Fee == 0),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "Should create a transfer in transaction");
    }

    #region Test Fixtures

    // Fixture class for test data generation
    public class Fixture
    {
        private readonly User _defaultUser;
        private readonly Cryptocurrency _defaultCrypto;
        private readonly Random _random;

        public Fixture()
        {
            _defaultUser = new User("test@example.com", "hashedpassword");
            _defaultCrypto = new Cryptocurrency("BTC", "Bitcoin");
            _random = new Random();
        }

        public Builder<T> Build<T>() where T : class
        {
            return new Builder<T>(this);
        }

        public T? Create<T>() where T : class
        {
            if (typeof(T) == typeof(Wallet))
            {
                var address = "0x" + Guid.NewGuid().ToString("N").Substring(0, 40);
                var wallet = new Wallet(
                    _defaultUser,
                    _defaultCrypto,
                    address
                );
                wallet.Deposit(10m, "Initial deposit"); // Initial balance
                return (T)(object)wallet;
            }

            if (typeof(T) == typeof(Transaction))
            {
                // Create a wallet first
                var wallet = new Wallet(
                    _defaultUser,
                    _defaultCrypto,
                    "0x" + Guid.NewGuid().ToString("N").Substring(0, 40));
                
                // Create a transaction with the wallet object
                return (T)(object)new Transaction(
                    wallet,
                    TransactionTypeEnum.Deposit,
                    _random.Next(1, 1000),
                    0.001m, // fee
                    "BTC",
                    null,
                    null,
                    "Test transaction");
            }

            if (typeof(T) == typeof(TransferRequest))
            {
                return (T)(object)new TransferRequest
                {
                    SourceWalletAddress = "0x" + Guid.NewGuid().ToString("N").Substring(0, 40),
                    DestinationWalletAddress = "0x" + Guid.NewGuid().ToString("N").Substring(0, 40),
                    Amount = 1.0m,
                    Fee = 0.001m,
                    CurrencyCode = "BTC",
                    Notes = "Test transfer"
                };
            }

            return default;
        }
    }

    public class Builder<T> where T : class
    {
        private readonly Fixture _fixture;
        private readonly T _item;

        public Builder(Fixture fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            _item = _fixture.Create<T>() ?? throw new InvalidOperationException($"Failed to create instance of {typeof(T).Name}");
        }

        public Builder<T> With<TValue>(string propertyName, TValue value)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(_item, value);
            }
            return this;
        }

        public T Create() => _item;
    }

    #endregion
    #endregion
}
