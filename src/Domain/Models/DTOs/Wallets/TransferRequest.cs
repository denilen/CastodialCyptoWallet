using System.ComponentModel.DataAnnotations;

namespace CryptoWallet.Domain.Models.DTOs.Wallets;

/// <summary>
/// Request DTO for transferring funds between wallets
/// </summary>
public class TransferRequest
{
    /// <summary>
    /// The source wallet address to transfer funds from
    /// </summary>
    [Required(ErrorMessage = "Source wallet address is required")]
    [StringLength(100, MinimumLength = 10,
        ErrorMessage = "Source wallet address must be between 10 and 100 characters")]
    public string SourceWalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// The destination wallet address to transfer funds to
    /// </summary>
    [Required(ErrorMessage = "Destination wallet address is required")]
    [StringLength(100, MinimumLength = 10,
        ErrorMessage = "Destination wallet address must be between 10 and 100 characters")]
    public string DestinationWalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// The amount to transfer (in the smallest unit of the cryptocurrency, e.g., satoshis for BTC, wei for ETH)
    /// </summary>
    [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// The cryptocurrency code (e.g., "BTC", "ETH") - used for validation
    /// </summary>
    [Required(ErrorMessage = "Currency code is required")]
    [StringLength(10, ErrorMessage = "Currency code cannot be longer than 10 characters")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Network fee to be paid for the transaction (if applicable)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Fee cannot be negative")]
    public decimal? Fee { get; set; }

    /// <summary>
    /// Additional notes or reference for the transfer
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// IP address of the user initiating the transfer
    /// </summary>
    [StringLength(45, ErrorMessage = "IP address cannot be longer than 45 characters")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the client making the request
    /// </summary>
    [StringLength(500, ErrorMessage = "User agent cannot be longer than 500 characters")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Two-factor authentication code (if required)
    /// </summary>
    [StringLength(10, ErrorMessage = "2FA code cannot be longer than 10 characters")]
    public string? TwoFactorCode { get; set; }
}
