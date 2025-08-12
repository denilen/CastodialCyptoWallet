using System.Text.Json.Serialization;

namespace CryptoWallet.Application.Wallets;

/// <summary>
/// Internal request model for depositing funds to a user's wallet
/// </summary>
public class DepositToUserRequest
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
    /// Amount to deposit
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Transaction hash from the blockchain (optional)
    /// </summary>
    public string? TransactionHash { get; set; }
    
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
