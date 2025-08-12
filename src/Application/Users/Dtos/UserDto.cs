namespace CryptoWallet.Application.Users.Dtos;

/// <summary>
/// User data transfer object
/// </summary>
public class UserDto
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's full name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// User's phone number (if provided)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User's country code (ISO 3166-1 alpha-2)
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Indicates if the user's email is confirmed
    /// </summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>
    /// Date and time when the user was created (UTC)
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the user last logged in (UTC)
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>
    /// Indicates if the user is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// List of user's wallet addresses
    /// </summary>
    public IReadOnlyList<WalletDto> Wallets { get; set; } = new List<WalletDto>();
}
