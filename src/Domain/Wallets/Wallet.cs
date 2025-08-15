using Ardalis.GuardClauses;
using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Currencies;
using CryptoWallet.Domain.Transactions;
using CryptoWallet.Domain.Users;

namespace CryptoWallet.Domain.Wallets;

/// <summary>
/// User's wallet for storing cryptocurrency
/// </summary>
public class Wallet : AuditableEntity
{
    /// <summary>
    /// The unique identifier of the wallet owner
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The user who owns this wallet
    /// </summary>
    public virtual User User { get; private set; } = null!;

    /// <summary>
    /// The unique identifier of the cryptocurrency
    /// </summary>
    public Guid CryptocurrencyId { get; private set; }

    /// <summary>
    /// The cryptocurrency stored in this wallet
    /// </summary>
    public virtual Cryptocurrency Cryptocurrency { get; private set; } = null!;

    /// <summary>
    /// The wallet address (unique identifier in the blockchain)
    /// </summary>
    public string Address { get; private set; }

    /// <summary>
    /// The current balance of the wallet
    /// </summary>
    public decimal Balance { get; private set; }

    /// <summary>
    /// Indicates whether the wallet is active
    /// </summary>
    public bool IsActive { get; private set; }

    // Private constructor for EF Core
    private Wallet()
    {
    }

    public Wallet(
        User user,
        Cryptocurrency cryptocurrency,
        string address)
        : base(Guid.NewGuid())
    {
        User = Guard.Against.Null(user, nameof(user));
        UserId = user.Id;
        Cryptocurrency = Guard.Against.Null(cryptocurrency, nameof(cryptocurrency));
        CryptocurrencyId = cryptocurrency.Id;
        Address = Guard.Against.NullOrWhiteSpace(address, nameof(address));
        Balance = 0;
        IsActive = true;

        // Initialize the collections to avoid null reference exceptions
        // The actual Wallet instances will be added by EF Core when loading from the database
        if (cryptocurrency.Wallets == null)
        {
            cryptocurrency.GetType().GetProperty("Wallets")?.SetValue(cryptocurrency, new List<Wallet>());
        }

        if (user.Wallets == null)
        {
            user.GetType().GetProperty("Wallets")?.SetValue(user, new List<Wallet>());
        }
    }

    /// <summary>
    /// Deposits funds into the wallet
    /// </summary>
    /// <param name="amount">The amount to deposit (must be positive)</param>
    /// <param name="modifiedBy">The ID of the user who initiated the operation</param>
    /// <returns>The new wallet balance</returns>
    public decimal Deposit(decimal amount, string? modifiedBy = null)
    {
        Guard.Against.NegativeOrZero(amount, nameof(amount));

        Balance += amount;
        UpdateAuditFields(modifiedBy);

        return Balance;
    }

    /// <summary>
    /// Withdraws funds from the wallet
    /// </summary>
    /// <param name="amount">The amount to withdraw (must be positive and not exceed the balance)</param>
    /// <param name="modifiedBy">The ID of the user who initiated the operation</param>
    /// <returns>The new wallet balance</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are insufficient funds</exception>
    public decimal Withdraw(decimal amount, string? modifiedBy = null)
    {
        Guard.Against.NegativeOrZero(amount, nameof(amount));

        if (Balance < amount)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        Balance -= amount;
        UpdateAuditFields(modifiedBy);

        return Balance;
    }

    /// <summary>
    /// Activates the wallet
    /// </summary>
    public void Activate(string? modifiedBy = null)
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Deactivates the wallet
    /// </summary>
    public void Deactivate(string? modifiedBy = null)
    {
        if (!IsActive)
            return;

        if (Balance > 0)
        {
            throw new InvalidOperationException("Cannot deactivate wallet with non-zero balance");
        }

        IsActive = false;
        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Collection of transactions associated with this wallet
    /// </summary>
    private readonly List<Transaction> _transactions = new();

    public virtual IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    /// <summary>
    /// Checks if the wallet has sufficient funds for a given amount
    /// </summary>
    public bool HasSufficientFunds(decimal amount)
    {
        return Balance >= amount;
    }
}
