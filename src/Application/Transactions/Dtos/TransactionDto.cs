using System.Text.Json.Serialization;

namespace CryptoWallet.Application.Transactions.Dtos;

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
    /// Type of the transaction (Deposit, Withdrawal, TransferIn, TransferOut, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the transaction (Pending, Completed, Failed, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Wallet address associated with this transaction
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Related wallet address (for transfers)
    /// </summary>
    public string? RelatedWalletAddress { get; set; }

    /// <summary>
    /// Amount involved in the transaction
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Fee charged for the transaction (if any)
    /// </summary>
    public decimal Fee { get; set; }

    /// <summary>
    /// Net amount (Amount - Fee for withdrawals, Amount for deposits)
    /// </summary>
    public decimal NetAmount => Type == "Withdrawal" ? Amount - Fee : Amount;

    /// <summary>
    /// Cryptocurrency code (e.g., "BTC", "ETH")
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
    /// Date and time when the transaction was confirmed (if applicable)
    /// </summary>
    public DateTimeOffset? ConfirmedAt { get; set; }

    /// <summary>
    /// Number of confirmations (for blockchain transactions)
    /// </summary>
    public int? Confirmations { get; set; }

    /// <summary>
    /// IP address of the user who initiated the transaction
    /// </summary>
    [JsonIgnore] // Don't expose in API responses by default
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of the client that initiated the transaction
    /// </summary>
    [JsonIgnore] // Don't expose in API responses by default
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional metadata as JSON (for extensibility)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Related transaction ID (for transfers between wallets)
    /// </summary>
    public Guid? RelatedTransactionId { get; set; }
}
