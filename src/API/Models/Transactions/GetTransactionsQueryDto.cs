using System.ComponentModel.DataAnnotations;

namespace CryptoWallet.API.Models.Transactions;

/// <summary>
/// Параметры запроса для получения списка транзакций
/// </summary>
public class GetTransactionsQueryDto
{
    /// <summary>
    /// Адрес кошелька (опционально)
    /// </summary>
    public string? WalletAddress { get; set; }

    /// <summary>
    /// Статус транзакции (опционально)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Тип транзакции (опционально)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Минимальная сумма (опционально)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Минимальная сумма не может быть отрицательной")]
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Максимальная сумма (опционально)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Максимальная сумма не может быть отрицательной")]
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Начальная дата для фильтрации (опционально)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Конечная дата для фильтрации (опционально)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Номер страницы (по умолчанию 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть положительным числом")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Размер страницы (по умолчанию 20, максимум 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "Размер страницы должен быть от 1 до 100")]
    public int PageSize { get; set; } = 20;
}
