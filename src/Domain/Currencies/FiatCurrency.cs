using Ardalis.GuardClauses;
using CryptoWallet.Domain.Common;

namespace CryptoWallet.Domain.Currencies;

/// <summary>
/// Represents a fiat currency (e.g., USD, EUR, RUB)
/// </summary>
public class FiatCurrency : AuditableEntity
{
    /// <summary>
    /// Currency code (e.g., "USD", "EUR", "RUB")
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Currency name (e.g., "US Dollar", "Euro", "Russian Ruble")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Currency symbol (e.g., "$", "€", "₽")
    /// </summary>
    public string Symbol { get; private set; }

    /// <summary>
    /// Number of decimal places to display for the currency
    /// </summary>
    public int DecimalPlaces { get; private set; }

    /// <summary>
    /// Indicates whether the currency is active for use
    /// </summary>
    public bool IsActive { get; private set; }

    // Private constructor for EF Core
    private FiatCurrency()
    {
    }

    public FiatCurrency(
        string code,
        string name,
        string symbol,
        int decimalPlaces = 2,
        bool isActive = true)
        : base(Guid.NewGuid())
    {
        Code = Guard.Against.NullOrWhiteSpace(code, nameof(code)).ToUpperInvariant();
        Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Symbol = Guard.Against.NullOrWhiteSpace(symbol, nameof(symbol));
        DecimalPlaces = Guard.Against.NegativeOrZero(decimalPlaces, nameof(decimalPlaces));
        IsActive = isActive;
    }

    /// <summary>
    /// Activates the currency
    /// </summary>
    /// <param name="modifiedBy">ID of the user who made the change (optional)</param>
    public void Activate(string? modifiedBy = null)
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Deactivates the currency
    /// </summary>
    /// <param name="modifiedBy">ID of the user who made the change (optional)</param>
    public void Deactivate(string? modifiedBy = null)
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Updates the currency information
    /// </summary>
    /// <param name="name">New name for the currency (optional)</param>
    /// <param name="symbol">New currency symbol (optional)</param>
    /// <param name="decimalPlaces">New number of decimal places (optional)</param>
    /// <param name="modifiedBy">ID of the user who made the change (optional)</param>
    public void UpdateInfo(
        string? name = null,
        string? symbol = null,
        int? decimalPlaces = null,
        string? modifiedBy = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (!string.IsNullOrWhiteSpace(symbol))
            Symbol = symbol;

        if (decimalPlaces.HasValue && decimalPlaces > 0)
            DecimalPlaces = decimalPlaces.Value;

        UpdateAuditFields(modifiedBy);
    }
}
