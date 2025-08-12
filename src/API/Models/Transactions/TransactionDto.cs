using System.Text.Json.Serialization;
using CryptoWallet.Domain.Enums;

namespace CryptoWallet.API.Models.Transactions;

/// <summary>
/// Represents a transaction data transfer object
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// Unique identifier of the transaction
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the wallet associated with the transaction
    /// </summary>
    public Guid WalletId { get; set; }

    /// <summary>
    /// Address of the wallet associated with the transaction
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Type of the transaction
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Status of the transaction
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Amount of the transaction (positive for deposits, negative for withdrawals)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Absolute amount value (always positive)
    /// </summary>
    [JsonIgnore]
    public decimal AbsoluteAmount => Math.Abs(Amount);

    /// <summary>
    /// Indicates if the transaction is a deposit
    /// </summary>
    [JsonIgnore]
    public bool IsDeposit => Amount > 0;

    /// <summary>
    /// Commission for the transaction
    /// </summary>
    public decimal Fee { get; set; }

    /// <summary>
    /// Currency of the commission
    /// </summary>
    public string FeeCurrency { get; set; } = string.Empty;

    /// <summary>
    /// Sender's address (if applicable)
    /// </summary>
    public string? FromAddress { get; set; }

    /// <summary>
    /// Recipient's address (if applicable)
    /// </summary>
    public string? ToAddress { get; set; }

    /// <summary>
    /// Transaction hash in the blockchain (if applicable)
    /// </summary>
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Transaction ID in the external system
    /// </summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Transaction description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date and time when the transaction was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the transaction was last updated
    /// </summary>
    public DateTimeOffset? LastModifiedAt { get; set; }

    /// <summary>
    /// Additional metadata in JSON format
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Identifier of the related transaction (for transfers between wallets)
    /// </summary>
    public Guid? RelatedTransactionId { get; set; }

    /// <summary>
    /// Creates a new instance of TransactionDto from a domain Transaction
    /// </summary>
    public static TransactionDto FromDomain(Domain.Transactions.Transaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        return new TransactionDto
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId,
            WalletAddress = transaction.Wallet?.Address ?? string.Empty,
            Type = transaction.TypeEnum.ToString(),
            Status = transaction.StatusEnum.ToString(),
            Amount = transaction.Amount,
            Fee = transaction.Fee,
            FeeCurrency = transaction.FeeCurrency,
            FromAddress = transaction.FromAddress,
            ToAddress = transaction.ToAddress,
            TransactionHash = transaction.TransactionHash,
            ExternalTransactionId = transaction.ExternalTransactionId,
            Description = transaction.Description,
            CreatedAt = transaction.CreatedAt,
            LastModifiedAt = transaction.LastModifiedAt,
            Metadata = transaction.Metadata,
            RelatedTransactionId = transaction.RelatedTransactionId
        };
    }

    /// <summary>
    /// Creates a new instance of TransactionDto from a domain Transaction with related transaction info
    /// </summary>
    public static TransactionDto FromDomainWithRelated(Domain.Transactions.Transaction transaction)
    {
        var dto = FromDomain(transaction);
        
        // Add related transaction info if exists
        if (transaction.RelatedTransaction != null)
        {
            dto.Description = $"{dto.Description} (Related to: {transaction.RelatedTransaction.Id})";
        }
        
        return dto;
    }
}
