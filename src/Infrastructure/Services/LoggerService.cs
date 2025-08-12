using CryptoWallet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.Infrastructure.Services;

/// <summary>
/// Implementation of the logger service using Microsoft.Extensions.Logging
/// </summary>
/// <typeparam name="T">The type to be used for the logger category</typeparam>
public class LoggerService<T> : ILoggerService<T> where T : class
{
    private readonly ILogger<T> _logger;

    public LoggerService(ILogger<T> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public void LogDebug(string message, params object[] args)
    {
        if (IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(message, args);
        }
    }

    /// <inheritdoc />
    public void LogInformation(string message, params object[] args)
    {
        if (IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(message, args);
        }
    }

    /// <inheritdoc />
    public void LogWarning(string message, params object[] args)
    {
        if (IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning(message, args);
        }
    }

    /// <inheritdoc />
    public void LogError(Exception exception, string message, params object[] args)
    {
        if (IsEnabled(LogLevel.Error))
        {
            _logger.LogError(exception, message, args);
        }
    }

    /// <inheritdoc />
    public void LogCritical(Exception exception, string message, params object[] args)
    {
        if (IsEnabled(LogLevel.Critical))
        {
            _logger.LogCritical(exception, message, args);
        }
    }

    /// <inheritdoc />
    public void LogTrace(string message, params object[] args)
    {
        if (IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(message, args);
        }
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return _logger.BeginScope(state);
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return _logger.IsEnabled(logLevel);
    }
}
