using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoWallet.API.Models.Wallets;

/// <summary>
/// Request model for withdrawing funds from a user's wallet
/// </summary>
public class WithdrawFromUserWalletRequest
{
    /// <summary>
    /// Currency code (e.g., "BTC", "ETH")
    /// </summary>
    [Required(ErrorMessage = "Currency code is required")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Currency code must be between 2 and 10 characters")]
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Amount to withdraw
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Destination wallet address for the withdrawal
    /// </summary>
    [Required(ErrorMessage = "Destination address is required")]
    [StringLength(100, MinimumLength = 20, ErrorMessage = "Destination address must be between 20 and 100 characters")]
    [JsonPropertyName("destinationAddress")]
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>
    /// Transaction fee (optional, will be calculated if not provided)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Fee cannot be negative")]
    [JsonPropertyName("fee")]
    public decimal? Fee { get; set; }

    /// <summary>
    /// Additional notes or reference (optional)
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
