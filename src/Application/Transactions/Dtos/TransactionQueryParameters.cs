using System.ComponentModel.DataAnnotations;

namespace CryptoWallet.Application.Transactions.Dtos;

/// <summary>
/// Query parameters for filtering and paginating transactions
/// </summary>
public class TransactionQueryParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;
    private string? _walletAddress;
    private string? _status;
    private string? _type;
    private string? _currencyCode;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page (1-100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Min(value, MaxPageSize);
    }

    /// <summary>
    /// Filter by wallet address (exact match)
    /// </summary>
    [StringLength(100, ErrorMessage = "Wallet address cannot be longer than 100 characters")]
    public string? WalletAddress
    {
        get => _walletAddress;
        set => _walletAddress = !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
    }

    /// <summary>
    /// Filter by status (exact match, case insensitive)
    /// </summary>
    [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters")]
    public string? Status
    {
        get => _status;
        set => _status = !string.IsNullOrWhiteSpace(value) ? value.Trim().ToLowerInvariant() : null;
    }

    /// <summary>
    /// Filter by transaction type (exact match, case insensitive)
    /// </summary>
    [StringLength(50, ErrorMessage = "Type cannot be longer than 50 characters")]
    public string? Type
    {
        get => _type;
        set => _type = !string.IsNullOrWhiteSpace(value) ? value.Trim().ToLowerInvariant() : null;
    }

    /// <summary>
    /// Filter by currency code (exact match, case insensitive)
    /// </summary>
    [StringLength(10, ErrorMessage = "Currency code cannot be longer than 10 characters")]
    public string? CurrencyCode
    {
        get => _currencyCode;
        set => _currencyCode = !string.IsNullOrWhiteSpace(value) ? value.Trim().ToUpperInvariant() : null;
    }

    /// <summary>
    /// Filter by minimum amount
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Minimum amount cannot be negative")]
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Filter by maximum amount
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Maximum amount cannot be negative")]
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Filter by start date (inclusive)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Filter by end date (inclusive)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Sort field (e.g., "Amount", "CreatedAt")
    /// </summary>
    [StringLength(50, ErrorMessage = "Sort field cannot be longer than 50 characters")]
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction ("asc" or "desc")
    /// </summary>
    [RegularExpression("^(?i)(asc|desc)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
    public string? SortDirection { get; set; } = "desc";

    /// <summary>
    /// Validates the query parameters
    /// </summary>
    /// <returns>True if valid, false otherwise with error message</returns>
    public (bool IsValid, string? ErrorMessage) Validate()
    {
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            return (false, "Start date cannot be after end date");

        if (MinAmount.HasValue && MaxAmount.HasValue && MinAmount > MaxAmount)
            return (false, "Minimum amount cannot be greater than maximum amount");

        if (StartDate > DateTime.UtcNow)
            return (false, "Start date cannot be in the future");

        if (EndDate > DateTime.UtcNow)
            return (false, "End date cannot be in the future");

        return (true, null);
    }
}
