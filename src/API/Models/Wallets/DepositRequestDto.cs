using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CryptoWallet.API.Models.Wallets;

/// <summary>
/// Запрос на пополнение кошелька
/// </summary>
public class DepositRequestDto
{
    /// <summary>
    /// Сумма для пополнения
    /// </summary>
    [Required(ErrorMessage = "Сумма пополнения обязательна")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "Сумма пополнения должна быть больше нуля")]
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Хеш транзакции в блокчейне (опционально)
    /// </summary>
    [StringLength(128, ErrorMessage = "Хеш транзакции не может превышать 128 символов")]
    [JsonPropertyName("transactionHash")]
    public string? TransactionHash { get; set; }

    /// <summary>
    /// Дополнительные примечания (опционально)
    /// </summary>
    [StringLength(500, ErrorMessage = "Примечание не может превышать 500 символов")]
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
