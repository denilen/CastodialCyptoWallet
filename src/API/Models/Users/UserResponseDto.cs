using System.Text.Json.Serialization;

namespace CryptoWallet.API.Models.Users;

/// <summary>
/// Ответ с информацией о пользователе
/// </summary>
public class UserResponseDto
{
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя (может быть null)
    /// </summary>
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Фамилия пользователя (может быть null)
    /// </summary>
    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    /// <summary>
    /// Дата и время создания аккаунта
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Признак подтвержденного email
    /// </summary>
    [JsonPropertyName("emailConfirmed")]
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Список кошельков пользователя
    /// </summary>
    [JsonPropertyName("wallets")]
    public ICollection<WalletDto> Wallets { get; set; } = new List<WalletDto>();
}

/// <summary>
/// DTO для кошелька пользователя
/// </summary>
public class WalletDto
{
    /// <summary>
    /// Адрес кошелька
    /// </summary>
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Код криптовалюты (например, "BTC", "ETH")
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Текущий баланс
    /// </summary>
    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Дата и время создания кошелька
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Статус кошелька (активен/неактивен)
    /// </summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}
