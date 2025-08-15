using Ardalis.GuardClauses;
using CryptoWallet.Domain.Common;
using CryptoWallet.Domain.Wallets;

namespace CryptoWallet.Domain.Users;

/// <summary>
/// Represents a system user
/// </summary>
public class User : AuditableEntity
{
    /// <summary>
    /// User's email address (unique identifier)
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Hashed user password
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; private set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public string? LastName { get; private set; }

    /// <summary>
    /// Indicates whether the user's email has been confirmed
    /// </summary>
    public bool IsEmailConfirmed { get; private set; }

    /// <summary>
    /// Date and time of the user's last login
    /// </summary>
    public DateTimeOffset? LastLoginDate { get; private set; }

    /// <summary>
    /// Collection of user's wallets
    /// </summary>
    private readonly List<Wallet> _wallets = new();

    public virtual IReadOnlyCollection<Wallet> Wallets => _wallets.AsReadOnly();

    // Private constructor for EF Core
    private User()
    {
    }

    public User(
        string email,
        string passwordHash,
        string? firstName = null,
        string? lastName = null)
        : base(Guid.NewGuid())
    {
        Email = Guard.Against.NullOrWhiteSpace(email, nameof(email));
        PasswordHash = Guard.Against.NullOrWhiteSpace(passwordHash, nameof(passwordHash));
        FirstName = firstName;
        LastName = lastName;
        IsEmailConfirmed = false;
    }

    /// <summary>
    /// Updates the user's basic information
    /// </summary>
    /// <param name="firstName">New first name (optional)</param>
    /// <param name="lastName">New last name (optional)</param>
    /// <param name="modifiedBy">ID of the user who made the change (optional)</param>
    public void UpdateInfo(string? firstName, string? lastName, string? modifiedBy = null)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName;

        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName;

        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Updates the user's password
    /// </summary>
    /// <param name="newPasswordHash">The new hashed password</param>
    /// <param name="modifiedBy">ID of the user who made the change (optional)</param>
    public void UpdatePassword(string newPasswordHash, string? modifiedBy = null)
    {
        PasswordHash = Guard.Against.NullOrWhiteSpace(newPasswordHash, nameof(newPasswordHash));
        UpdateAuditFields(modifiedBy);
    }

    /// <summary>
    /// Confirms the user's email address
    /// </summary>
    public void ConfirmEmail()
    {
        if (IsEmailConfirmed)
            return;

        IsEmailConfirmed = true;
        UpdateAuditFields();
    }

    /// <summary>
    /// Updates the last login timestamp to the current UTC time
    /// </summary>
    public void UpdateLastLoginDate()
    {
        LastLoginDate = DateTimeOffset.UtcNow;
    }
}
