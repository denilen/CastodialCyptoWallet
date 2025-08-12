using System.ComponentModel.DataAnnotations;

namespace CryptoWallet.API.Models.Users;

/// <summary>
/// Запрос на регистрацию нового пользователя
/// </summary>
public class RegisterUserRequestDto
{
    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [StringLength(100, ErrorMessage = "Email не может быть длиннее 100 символов")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль пользователя
    /// </summary>
    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен содержать от 8 до 100 символов")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждение пароля
    /// </summary>
    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя (опционально)
    /// </summary>
    [StringLength(50, ErrorMessage = "Имя не может быть длиннее 50 символов")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия пользователя (опционально)
    /// </summary>
    [StringLength(50, ErrorMessage = "Фамилия не может быть длиннее 50 символов")]
    public string? LastName { get; set; }
}
