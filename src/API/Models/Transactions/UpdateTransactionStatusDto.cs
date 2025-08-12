using System.ComponentModel.DataAnnotations;

namespace CryptoWallet.API.Models.Transactions;

/// <summary>
/// Запрос на обновление статуса транзакции
/// </summary>
public class UpdateTransactionStatusDto
{
    /// <summary>
    /// Новый статус транзакции
    /// </summary>
    [Required(ErrorMessage = "Статус транзакции обязателен")]
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Дополнительные примечания (опционально)
    /// </summary>
    [StringLength(500, ErrorMessage = "Примечание не может превышать 500 символов")]
    public string? Notes { get; set; }
}
