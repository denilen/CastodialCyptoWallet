using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoWallet.API.Models.Wallets;

/// <summary>
/// Request model for depositing funds to a user's wallet
/// </summary>
public class DepositToUserWalletRequest
{
    /// <summary>
    /// Currency code (e.g., "BTC", "ETH")
    /// </summary>
    [Required(ErrorMessage = "Currency code is required")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Currency code must be between 2 and 10 characters")]
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Amount to deposit
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction hash from the blockchain (optional)
    /// </summary>
    [StringLength(128, ErrorMessage = "Transaction hash cannot exceed 128 characters")]
    [JsonPropertyName("transactionHash")]
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Additional notes or reference (optional)
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
