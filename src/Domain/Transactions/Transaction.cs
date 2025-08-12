using Ardalis.GuardClauses;
using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Enums;
using CryptoWallet.Domain.Wallets;

namespace CryptoWallet.Domain.Transactions;

/// <summary>
/// Represents a transaction in the system
/// </summary>
public class Transaction : AuditableEntity
{
    /// <summary>
    /// Identifier of the wallet associated with the transaction
    /// </summary>
    public Guid WalletId { get; private set; }

    /// <summary>
    /// Wallet associated with the transaction
    /// </summary>
    public virtual Wallet Wallet { get; private set; } = null!;

    /// <summary>
    /// Identifier of the related transaction (e.g. for transfers between wallets)
    /// </summary>
    public Guid? RelatedTransactionId { get; private set; }

    /// <summary>
    /// Related transaction
    /// </summary>
    public virtual Transaction? RelatedTransaction { get; private set; }

    /// <summary>
    /// Type of transaction
    /// </summary>
    public TransactionTypeEnum TypeEnum { get; private set; }

    /// <summary>
    /// Status of the transaction
    /// </summary>
    public TransactionStatusEnum StatusEnum { get; private set; }

    /// <summary>
    /// Amount of the transaction (positive for deposits, negative for withdrawals)
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Commission for the transaction
    /// </summary>
    public decimal Fee { get; private set; }

    /// <summary>
    /// Currency of the commission
    /// </summary>
    public string FeeCurrency { get; private set; } = string.Empty;

    /// <summary>
    /// Sender's address (if applicable)
    /// </summary>
    public string? FromAddress { get; private set; }

    /// <summary>
    /// Адрес получателя (если применимо)
    /// </summary>
    public string? ToAddress { get; private set; }

    /// <summary>
    /// Хеш транзакции в блокчейне (если применимо)
    /// </summary>
    public string? TransactionHash { get; private set; }

    /// <summary>
    /// Идентификатор транзакции во внешней системе
    /// </summary>
    public string? ExternalTransactionId { get; private set; }

    /// <summary>
    /// Описание транзакции
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Дополнительные метаданные в формате JSON
    /// </summary>
    public string? Metadata { get; private set; }

    // Private constructor for EF Core
    private Transaction() { }

    public Transaction(
        Wallet wallet,
        TransactionTypeEnum typeEnum,
        decimal amount,
        decimal fee,
        string feeCurrency,
        string? fromAddress = null,
        string? toAddress = null,
        string? description = null,
        string? metadata = null)
        : base(Guid.NewGuid())
    {
        Wallet = Guard.Against.Null(wallet, nameof(wallet));
        WalletId = wallet.Id;
        TypeEnum = typeEnum;
        StatusEnum = TransactionStatusEnum.Pending;
        Amount = amount != 0 ? amount : throw new ArgumentException("Amount cannot be zero", nameof(amount));
        Fee = Guard.Against.Negative(fee, nameof(fee));
        FeeCurrency = Guard.Against.NullOrWhiteSpace(feeCurrency, nameof(feeCurrency));
        FromAddress = fromAddress;
        ToAddress = toAddress;
        Description = description;
        Metadata = metadata;
    }

    /// <summary>
    /// Marks the transaction as being processed
    /// </summary>
    public void MarkAsProcessing(string? modifiedBy = null)
    {
        if (StatusEnum != TransactionStatusEnum.Pending)
        {
            throw new InvalidOperationException("Only pending transactions can be marked as processing");
        }

        StatusEnum = TransactionStatusEnum.Processing;
        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Marks the transaction as successfully completed
    /// </summary>
    public void MarkAsCompleted(string? transactionHash = null, string? modifiedBy = null)
    {
        if (StatusEnum != TransactionStatusEnum.Processing && StatusEnum != TransactionStatusEnum.Pending)
        {
            throw new InvalidOperationException("Only pending or processing transactions can be marked as completed");
        }

        StatusEnum = TransactionStatusEnum.Completed;

        if (!string.IsNullOrWhiteSpace(transactionHash))
        {
            TransactionHash = transactionHash;
        }

        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Marks the transaction as rejected
    /// </summary>
    public void MarkAsRejected(string reason, string? modifiedBy = null)
    {
        if (StatusEnum != TransactionStatusEnum.Pending && StatusEnum != TransactionStatusEnum.Processing)
        {
            throw new InvalidOperationException("Only pending or processing transactions can be rejected");
        }

        StatusEnum = TransactionStatusEnum.Rejected;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            Description = $"Rejected: {reason}";
        }

        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Marks the transaction as cancelled by user
    /// </summary>
    public void MarkAsCancelled(string? modifiedBy = null)
    {
        if (StatusEnum != TransactionStatusEnum.Pending && StatusEnum != TransactionStatusEnum.Processing)
        {
            throw new InvalidOperationException("Only pending or processing transactions can be cancelled");
        }

        StatusEnum = TransactionStatusEnum.Cancelled;
        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Marks the transaction as failed with an error
    /// </summary>
    public void MarkAsFailed(string error, string? modifiedBy = null)
    {
        if (StatusEnum != TransactionStatusEnum.Processing)
        {
            throw new InvalidOperationException("Only processing transactions can be marked as failed");
        }

        StatusEnum = TransactionStatusEnum.Failed;

        if (!string.IsNullOrWhiteSpace(error))
        {
            Description = $"Failed: {error}";
        }

        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Связывает транзакцию с другой транзакцией (например, для переводов между кошельками)
    /// </summary>
    public void LinkToRelatedTransaction(Transaction relatedTransaction, string? modifiedBy = null)
    {
        if (relatedTransaction == null)
            throw new ArgumentNullException(nameof(relatedTransaction));

        if (relatedTransaction.Id == Id)
            throw new InvalidOperationException("Cannot link a transaction to itself");

        RelatedTransaction = relatedTransaction;
        RelatedTransactionId = relatedTransaction.Id;
        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Обновляет хеш транзакции в блокчейне
    /// </summary>
    public void UpdateTransactionHash(string transactionHash, string? modifiedBy = null)
    {
        if (string.IsNullOrWhiteSpace(transactionHash))
            throw new ArgumentException("Transaction hash cannot be empty", nameof(transactionHash));

        TransactionHash = transactionHash;
        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Обновляет внешний идентификатор транзакции
    /// </summary>
    public void UpdateExternalTransactionId(string externalTransactionId, string? modifiedBy = null)
    {
        if (string.IsNullOrWhiteSpace(externalTransactionId))
            throw new ArgumentException("External transaction ID cannot be empty", nameof(externalTransactionId));

        ExternalTransactionId = externalTransactionId;
        UpdateAuditFields(modifiedBy);
    }
}
