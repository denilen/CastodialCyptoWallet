namespace CryptoWallet.Domain.Models.DTOs.Wallets;

/// <summary>
/// Wallet balance data transfer object
/// </summary>
public class WalletBalanceDto
{
    /// <summary>
    /// Wallet address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Cryptocurrency code (e.g., "BTC", "ETH", "USDT")
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Current available balance
    /// </summary>
    public decimal AvailableBalance { get; set; }

    /// <summary>
    /// Balance that is currently locked in pending transactions
    /// </summary>
    public decimal LockedBalance { get; set; }

    /// <summary>
    /// Total balance (Available + Locked)
    /// </summary>
    public decimal TotalBalance => AvailableBalance + LockedBalance;

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTimeOffset LastUpdated { get; set; }

    /// <summary>
    /// Value in USD (if available)
    /// </summary>
    public decimal? ValueInUsd { get; set; }

    /// <summary>
    /// Value in user's preferred fiat currency (if available)
    /// </summary>
    public decimal? ValueInFiat { get; set; }

    /// <summary>
    /// Fiat currency code (e.g., "USD", "EUR")
    /// </summary>
    public string? FiatCurrencyCode { get; set; }
}
