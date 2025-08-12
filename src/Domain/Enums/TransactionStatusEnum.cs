namespace CryptoWallet.Domain.Enums;

/// <summary>
/// Represents the status of a transaction
/// </summary>
public enum TransactionStatusEnum
{
    /// <summary>
    /// Transaction is pending processing
    /// </summary>
    Pending,

    /// <summary>
    /// Transaction is being processed
    /// </summary>
    Processing,

    /// <summary>
    /// Transaction completed successfully
    /// </summary>
    Completed,

    /// <summary>
    /// Transaction was rejected
    /// </summary>
    Rejected,

    /// <summary>
    /// Transaction was cancelled by user
    /// </summary>
    Cancelled,

    /// <summary>
    /// Transaction failed with an error
    /// </summary>
    Failed
}
