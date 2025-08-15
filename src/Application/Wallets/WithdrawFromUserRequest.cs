using System.Text.Json.Serialization;

namespace CryptoWallet.Application.Wallets;

/// <summary>
/// Internal request model for withdrawing funds from a user's wallet
/// </summary>
public class WithdrawFromUserRequest
{
    /// <summary>
    /// User ID
    /// </summary>
    [JsonIgnore] // Don't serialize/deserialize this property
    public Guid UserId { get; set; }

    /// <summary>
    /// Currency code (e.g., "BTC", "ETH")
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Amount to withdraw
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Destination wallet address for the withdrawal
    /// </summary>
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>
    /// Transaction fee (optional, will be calculated if not provided)
    /// </summary>
    public decimal? Fee { get; set; }

    /// <summary>
    /// Additional notes or reference (optional)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// IP address of the requestor (for audit)
    /// </summary>
    [JsonIgnore] // Don't serialize/deserialize this property
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent of the requestor (for audit)
    /// </summary>
    [JsonIgnore] // Don't serialize/deserialize this property
    public string? UserAgent { get; set; }
}
