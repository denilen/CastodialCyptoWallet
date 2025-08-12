using System.Text.Json.Serialization;

namespace CryptoWallet.Domain.Models.DTOs.Wallets;

/// <summary>
/// Wallet data transfer object
/// </summary>
public class WalletDto
{
    /// <summary>
    /// Unique identifier for the wallet
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// User ID that owns this wallet
    /// </summary>
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Cryptocurrency code (e.g., "BTC", "ETH", "USDT")
    /// </summary>
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Cryptocurrency name (e.g., "Bitcoin", "Ethereum")
    /// </summary>
    [JsonPropertyName("currencyName")]
    public string CurrencyName { get; set; } = string.Empty;

    /// <summary>
    /// Wallet address
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Current balance in the wallet
    /// </summary>
    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    /// <summary>
    /// Indicates if this is the default wallet for the currency
    /// </summary>
    [JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// Indicates if the wallet is active
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time when the wallet was created (UTC)
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the wallet was last updated (UTC)
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
