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

namespace CryptoWallet.UnitTests.Application.Services;

public class WalletServiceTests
{
    private readonly Mock<ILogger<WalletService>> _loggerMock;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<ICryptocurrencyRepository> _cryptocurrencyRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly WalletService _walletService;
    private readonly Fixture _fixture;

    public WalletServiceTests()
    {
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
            .ReturnsAsync((Wallet)null);

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
            20m, // amount
            0.001m, // fee
            "BTC", // feeCurrency
            wallet.Address, // fromAddress
            "0xrecipient", // toAddress
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
        result.Value.Should().NotBeNull();
        result.Value.AvailableBalance.Should().Be(80m); // 100 - 20 (pending withdrawal)
        result.Value.LockedBalance.Should().Be(20m); // pending withdrawal amount
        result.Value.TotalBalance.Should().Be(100m); // total balance remains the same
        result.Value.CurrencyCode.Should().Be("BTC");
        result.Value.Address.Should().Be(wallet.Address);
    }

    [Fact]
    public async Task WithdrawFundsAsync_WithSufficientBalance_ShouldSucceed()
    {
        // Arrange
        var walletAddress = "0x1234567890abcdef1234567890abcdef12345678";
        var user = new User("test@example.com", "hashedpassword");
        var crypto = new Cryptocurrency("BTC", "Bitcoin");
        var wallet = new Wallet(user, crypto, walletAddress);
        wallet.Deposit(100m, "Test deposit");
        var amount = 50m;
        var fee = 0.001m;

        var withdrawRequest = new WithdrawRequest
        {
            SourceWalletAddress = walletAddress,
            Amount = amount,
            Fee = fee,
            DestinationAddress = "0xrecipient",
            Notes = "Test withdrawal"
        };

        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(walletAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _walletRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _walletService.WithdrawFundsAsync(withdrawRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        wallet.Balance.Should().Be(49.999m); // 100 - 50 - 0.001 fee

        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Transaction>(t =>
                t.Amount == amount &&
                t.Fee == fee &&
                t.TypeEnum == TransactionTypeEnum.Withdrawal &&
                t.StatusEnum == TransactionStatusEnum.Completed),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TransferFundsAsync_WithSameSourceAndDestination_ShouldFail()
    {
        // Arrange
        var walletAddress = "0x1234567890abcdef1234567890abcdef12345678";
        var transferRequest = new TransferRequest
        {
            SourceWalletAddress = walletAddress,
            DestinationWalletAddress = walletAddress,
            Amount = 10m,
            Fee = 0.001m,
            Notes = "Test transfer",
            CurrencyCode = "BTC"
        };

        // Act
        var result = await _walletService.TransferFundsAsync(transferRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("same wallet"));
    }

    [Fact]
    public async Task DepositFundsAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var user = new User("test@example.com", "hashedpassword");
        var crypto = new Cryptocurrency("BTC", "Bitcoin");
        var wallet = new Wallet(user, crypto, "0x1234567890abcdef1234567890abcdef12345678");
        var amount = 10m;
        var fee = 0.001m;
        var description = "Test deposit";
        var depositRequest = new DepositRequest
        {
            WalletAddress = wallet.Address,
            Amount = amount,
            Notes = description,
            TransactionHash = "tx123",
            IpAddress = "127.0.0.1",
            UserAgent = "test-agent"
        };

        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(wallet.Address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        _walletRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _walletService.DepositFundsAsync(depositRequest);

        // Assert
        result.IsSuccess.Should().BeTrue($"Expected success but got error: {string.Join("; ", result.Errors)}");
        wallet.Balance.Should().Be(amount);
        _walletRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()), Times.Once);


    }

    [Fact]
    public async Task WithdrawFundsAsync_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var user = new User("test@example.com", "hashedpassword");
        var crypto = new Cryptocurrency("BTC", "Bitcoin");
        var wallet = new Wallet(user, crypto, "0x1234567890abcdef1234567890abcdef12345678");
        var amount = 1000m; // Large amount to ensure insufficient balance
        var fee = 0.001m;
        var description = "Test withdrawal";
        var withdrawRequest = new WithdrawRequest
        {
            SourceWalletAddress = wallet.Address,
            Amount = amount,
            Fee = fee,
            DestinationAddress = "0xabcdef1234567890abcdef1234567890abcdef12",
            Notes = description,
            CurrencyCode = "BTC"
        };

        wallet.Deposit(100m, "Initial deposit"); // Add some initial balance but less than withdrawal amount

        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(wallet.Address, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wallet);

        // Act
        var result = await _walletService.WithdrawFundsAsync(withdrawRequest);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Insufficient funds"));
        _walletRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TransferFundsAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var sourceAddress = "0x1234567890abcdef1234567890abcdef12345678";
        var destinationAddress = "0xabcdef1234567890abcdef1234567890abcdef12";
        var amount = 10m;
        var fee = 0.001m;
        var currencyCode = "BTC";

        var user = new User("test@example.com", "hashedpassword");
        var btc = new Cryptocurrency(currencyCode, "Bitcoin", 8);

        var sourceWallet = new Wallet(user, btc, sourceAddress);
        sourceWallet.Deposit(50m, "Initial deposit");

        var destinationWallet = new Wallet(user, btc, destinationAddress);
        destinationWallet.Deposit(20m, "Initial deposit");

        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(sourceAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceWallet);

        _walletRepositoryMock
            .Setup(x => x.GetByAddressWithDetailsAsync(destinationAddress, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationWallet);

        _walletRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _transactionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var transferRequest = new TransferRequest
        {
            SourceWalletAddress = sourceAddress,
            DestinationWalletAddress = destinationAddress,
            Amount = amount,
            Fee = fee,
            Notes = "Test transfer",
            CurrencyCode = currencyCode
        };

        // Act
        var result = await _walletService.TransferFundsAsync(transferRequest);

        // Assert
        result.IsSuccess.Should().BeTrue();
        sourceWallet.Balance.Should().Be(39.999m); // 50 - 10 - 0.001 fee
        destinationWallet.Balance.Should().Be(30m); // 20 + 10

        _walletRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Wallet>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _transactionRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Transaction>(t =>
                t.Amount == amount &&
                t.Fee == fee &&
                t.TypeEnum == TransactionTypeEnum.Transfer &&
                t.StatusEnum == TransactionStatusEnum.Completed),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWalletTransactionsAsync_WithValidRequest_ShouldReturnTransactions()
    {
        // This test should be moved to TransactionServiceTests.cs since GetWalletTransactionsAsync
        // is a method of TransactionService, not WalletService.
        // Please see TransactionServiceTests.cs for the implementation of this test.
        await Task.CompletedTask;
    }
}

// Fixture class for test data generation
public class Fixture
{
    private readonly Random _random = new();
    private readonly User _defaultUser;
    private readonly Cryptocurrency _defaultCrypto;

    public Fixture()
    {
        _defaultUser = new User("test@example.com", "hashedpassword");
        _defaultCrypto = new Cryptocurrency("BTC", "Bitcoin");
    }

    public Builder<T> Build<T>() where T : class
    {
        return new Builder<T>(this);
    }

    public T Create<T>()
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
        _fixture = fixture;
        _item = _fixture.Create<T>();
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
