using Microsoft.Extensions.Logging;

namespace CryptoWallet.Application.Common.Interfaces;

/// <summary>
/// Service for application-wide logging
/// </summary>
public interface ILoggerService<T> where T : class
{
    /// <summary>
    /// Logs a debug message
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs an informational message
    /// </summary>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Logs a warning message
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Logs an error message with an exception
    /// </summary>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs a critical error message with an exception
    /// </summary>
    void LogCritical(Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs a trace message
    /// </summary>
    void LogTrace(string message, params object[] args);

    /// <summary>
    /// Logs an operation with timing information
    /// </summary>
    IDisposable BeginScope<TState>(TState state);

    /// <summary>
    /// Checks if the specified log level is enabled
    /// </summary>
    bool IsEnabled(LogLevel logLevel);
}
