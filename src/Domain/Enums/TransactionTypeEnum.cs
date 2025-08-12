namespace CryptoWallet.Domain.Enums;

/// <summary>
/// Represents the type of a transaction
/// </summary>
public enum TransactionTypeEnum
{
    /// <summary>
    /// Deposit transaction (funding)
    /// </summary>
    Deposit,

    /// <summary>
    /// Withdrawal transaction
    /// </summary>
    Withdrawal,

    /// <summary>
    /// Transfer between wallets
    /// </summary>
    Transfer,

    /// <summary>
    /// Currency exchange/conversion
    /// </summary>
    Exchange,

    /// <summary>
    /// Fee charge
    /// </summary>
    Fee,

    /// <summary>
    /// Interest payment
    /// </summary>
    Interest,

    /// <summary>
    /// Refund transaction
    /// </summary>
    Refund,

    /// <summary>
    /// Other types of operations
    /// </summary>
    Other
}
