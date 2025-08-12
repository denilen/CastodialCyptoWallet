namespace CryptoWallet.Application.Wallets.Dtos;

/// <summary>
/// Transaction data transfer object
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// Unique identifier for the transaction
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Transaction type (Deposit, Withdrawal, Transfer, etc.)
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status (Pending, Completed, Failed, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The wallet address associated with this transaction
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// The related wallet address (for transfers)
    /// </summary>
    public string? RelatedWalletAddress { get; set; }

    /// <summary>
    /// The amount involved in the transaction
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The fee charged for the transaction (if any)
    /// </summary>
    public decimal Fee { get; set; }

    /// <summary>
    /// The cryptocurrency code (e.g., "BTC", "ETH")
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// The transaction hash from the blockchain (if applicable)
    /// </summary>
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Additional notes or reference for the transaction
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Date and time when the transaction was created (UTC)
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the transaction was last updated (UTC)
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// IP address of the user who initiated the transaction
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the client that initiated the transaction
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional metadata as JSON (for extensibility)
    /// </summary>
    public string? Metadata { get; set; }
}
