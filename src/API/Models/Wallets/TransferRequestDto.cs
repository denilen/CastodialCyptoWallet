using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoWallet.API.Models.Wallets;

/// <summary>
/// Запрос на перевод средств между кошельками
/// </summary>
public class TransferRequestDto
{
    /// <summary>
    /// Адрес кошелька отправителя
    /// </summary>
    [Required(ErrorMessage = "Адрес кошелька отправителя обязателен")]
    [StringLength(64, MinimumLength = 26, ErrorMessage = "Некорректный формат адреса кошелька отправителя")]
    [JsonPropertyName("sourceWalletAddress")]
    public string SourceWalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Адрес кошелька получателя
    /// </summary>
    [Required(ErrorMessage = "Адрес кошелька получателя обязателен")]
    [StringLength(64, MinimumLength = 26, ErrorMessage = "Некорректный формат адреса кошелька получателя")]
    [JsonPropertyName("destinationWalletAddress")]
    public string DestinationWalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Сумма перевода
    /// </summary>
    [Required(ErrorMessage = "Сумма перевода обязательна")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Сумма перевода должна быть больше нуля")]
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Комиссия за перевод (опционально)
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
