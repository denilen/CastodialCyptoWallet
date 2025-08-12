namespace CryptoWallet.Application.Users.Dtos;

/// <summary>
/// Wallet data transfer object
/// </summary>
public class WalletDto
{
    /// <summary>
    /// Unique identifier for the wallet
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Cryptocurrency code (e.g., "BTC", "ETH", "USDT")
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Cryptocurrency name (e.g., "Bitcoin", "Ethereum")
    /// </summary>
    public string CurrencyName { get; set; } = string.Empty;

    /// <summary>
    /// Wallet address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Current balance in the wallet
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Indicates if this is the default wallet for the currency
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Date and time when the wallet was created (UTC)
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
