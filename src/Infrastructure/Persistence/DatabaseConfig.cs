namespace CryptoWallet.Infrastructure.Persistence;

/// <summary>
/// Database configuration
/// </summary>
public class DatabaseConfig
{
    /// <summary>
    /// Host database
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Port database
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// The name of the database
    /// </summary>
    public string Database { get; set; } = "ccw";

    /// <summary>
    /// User name
    /// </summary>
    public string Username { get; set; } = "ccw";

    /// <summary>
    /// User password
    /// </summary>
    public string Password { get; set; } = "ccw";

    /// <summary>
    /// The maximum number of connections in the bullet
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Include detailed requests logistics
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Get a string of connection
    /// </summary>
    public string GetConnectionString()
    {
        return
            $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};Maximum Pool Size={MaxPoolSize};";
    }
}
