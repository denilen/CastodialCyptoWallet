using Ardalis.GuardClauses;
using CryptoWallet.Domain.Common;

namespace CryptoWallet.Domain.Currencies;

/// <summary>
/// Represents a cryptocurrency (e.g., BTC, ETH, USDT)
/// </summary>
public class Cryptocurrency : AuditableEntity
{
    /// <summary>
    /// Cryptocurrency code (e.g., "BTC", "ETH", "USDT")
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Cryptocurrency name (e.g., "Bitcoin", "Ethereum", "Tether")
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Number of decimal places to display for the cryptocurrency
    /// </summary>
    public int DecimalPlaces { get; private set; }

    /// <summary>
    /// Indicates whether the currency is active for use
    /// </summary>
    public bool IsActive { get; private set; }

    // Private constructor for EF Core
    private Cryptocurrency()
    {
    }

    public Cryptocurrency(
        string code,
        string name,
        int decimalPlaces = 8,
        bool isActive = true)
        : base(Guid.NewGuid())
    {
        Code = Guard.Against.NullOrWhiteSpace(code, nameof(code)).ToUpperInvariant();
        Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
        DecimalPlaces = Guard.Against.NegativeOrZero(decimalPlaces, nameof(decimalPlaces));
        IsActive = isActive;
    }

    /// <summary>
    /// Activates the cryptocurrency
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
    /// Deactivates the cryptocurrency
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
    /// Updates the cryptocurrency information
    /// </summary>
    /// <param name="name">New name for the cryptocurrency (optional)</param>
    /// <param name="decimalPlaces">New number of decimal places (optional)</param>
    /// <param name="modifiedBy">ID of the user who made the change (optional)</param>
    public void UpdateInfo(string? name = null, int? decimalPlaces = null, string? modifiedBy = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
            Name = name;

        if (decimalPlaces.HasValue && decimalPlaces > 0)
            DecimalPlaces = decimalPlaces.Value;

        UpdateAuditFields(modifiedBy);
    }
}
