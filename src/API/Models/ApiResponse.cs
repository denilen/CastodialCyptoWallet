using System.Text.Json.Serialization;

namespace CryptoWallet.API.Models;

/// <summary>
/// Базовый класс для всех ответов API
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Флаг успешности выполнения запроса
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    /// <summary>
    /// Данные ответа
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    /// <summary>
    /// Сообщение об ошибке (если есть)
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    /// <summary>
    /// Детали ошибок валидации (если есть)
    /// </summary>
    [JsonPropertyName("validationErrors")]
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    
    /// <summary>
    /// Временная метка ответа
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    
    /// <summary>
    /// Создает успешный ответ с данными
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }
    
    /// <summary>
    /// Создает ответ с ошибкой
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string error, Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error,
            ValidationErrors = validationErrors
        };
    }
}
