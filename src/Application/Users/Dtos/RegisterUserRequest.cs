using System.ComponentModel.DataAnnotations;

namespace CryptoWallet.Application.Users.Dtos;

/// <summary>
/// Request DTO for user registration
/// </summary>
public class RegisterUserRequest
{
    /// <summary>
    /// User's email address (must be unique)
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password (will be hashed before storage)
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// User's full name
    /// </summary>
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// User's phone number (optional)
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User's country code (ISO 3166-1 alpha-2)
    /// </summary>
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be 2 characters")]
    public string? CountryCode { get; set; }

    /// <summary>
    /// IP address of the user's registration
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from the registration request
    /// </summary>
    public string? UserAgent { get; set; }
}
