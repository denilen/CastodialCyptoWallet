using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoWallet.API.Models.Wallets;

/// <summary>
/// Запрос на вывод средств с кошелька
/// </summary>
public class WithdrawRequestDto
{
    /// <summary>
    /// Адрес кошелька для вывода средств
    /// </summary>
    [Required(ErrorMessage = "Адрес кошелька обязателен")]
    [StringLength(64, MinimumLength = 26, ErrorMessage = "Некорректный формат адреса кошелька")]
    [JsonPropertyName("destinationAddress")]
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>
    /// Сумма для вывода
    /// </summary>
    [Required(ErrorMessage = "Сумма вывода обязательна")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Сумма вывода должна быть больше нуля")]
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Комиссия за вывод (опционально)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Комиссия не может быть отрицательной")]
    [JsonPropertyName("fee")]
    public decimal? Fee { get; set; }

    /// <summary>
    /// Дополнительные примечания (опционально)
    /// </summary>
    [StringLength(500, ErrorMessage = "Примечание не может превышать 500 символов")]
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
