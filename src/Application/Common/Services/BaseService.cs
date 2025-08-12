using CryptoWallet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CryptoWallet.Application.Common.Services;

/// <summary>
/// Base class for all application services
/// </summary>
public abstract class BaseService : IService
{
    protected readonly ILogger Logger;

    protected BaseService(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs an information message
    /// </summary>
    protected void LogInformation(string message, params object[] args)
    {
        Logger.LogInformation(message, args);
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    protected void LogWarning(string message, params object[] args)
    {
        Logger.LogWarning(message, args);
    }

    /// <summary>
    /// Logs an error message
    /// </summary>
    protected void LogError(Exception exception, string message, params object[] args)
    {
        Logger.LogError(exception, message, args);
    }
}
