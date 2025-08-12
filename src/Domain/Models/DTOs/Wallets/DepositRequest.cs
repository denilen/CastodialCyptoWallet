using System.ComponentModel.DataAnnotations;

namespace CryptoWallet.Domain.Models.DTOs.Wallets;

/// <summary>
/// Request DTO for depositing funds into a wallet
/// </summary>
public class DepositRequest
{
    /// <summary>
    /// The wallet address to deposit funds into
    /// </summary>
    [Required(ErrorMessage = "Wallet address is required")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Wallet address must be between 10 and 100 characters")]
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// The amount to deposit (in the smallest unit of the cryptocurrency, e.g., satoshis for BTC, wei for ETH)
    /// </summary>
    [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// The transaction fee (in the smallest unit of the cryptocurrency)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Fee cannot be negative")]
    public decimal Fee { get; set; }

    /// <summary>
    /// The transaction hash from the blockchain (if applicable)
    /// </summary>
    [StringLength(100, ErrorMessage = "Transaction hash cannot be longer than 100 characters")]
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Additional notes or reference for the deposit
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// IP address of the user initiating the deposit
    /// </summary>
    [StringLength(45, ErrorMessage = "IP address cannot be longer than 45 characters")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the client making the request
    /// </summary>
    [StringLength(500, ErrorMessage = "User agent cannot be longer than 500 characters")]
    public string? UserAgent { get; set; }
}
