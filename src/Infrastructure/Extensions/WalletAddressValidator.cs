using System.Text.RegularExpressions;

namespace CryptoWallet.Infrastructure.Extensions;

/// <summary>
/// Provides validation for cryptocurrency wallet addresses
/// </summary>
public static class WalletAddressValidator
{
    // These constants should be configured based on your specific requirements
    private const int MinAddressLength = 26;  // Minimum length for most crypto addresses
    private const int MaxAddressLength = 100; // Maximum length to prevent potential DoS attacks

    // Common patterns for different cryptocurrencies (simplified examples)
    private static readonly (string Prefix, string Pattern)[] CryptoPatterns =
    {
        ("btc", "^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$"), // Bitcoin
        ("eth", "^0x[a-fA-F0-9]{40}$"),               // Ethereum
        ("usdt", "^(T|1)[a-km-zA-HJ-NP-Z1-9]{33}$"),  // Tether (Omni/TRC20/ERC20)
        ("xrp", "^r[0-9a-zA-Z]{24,34}$"),             // Ripple
        ("ltc", "^[LM3][a-km-zA-HJ-NP-Z1-9]{26,33}$") // Litecoin
    };

    /// <summary>
    /// Validates a cryptocurrency wallet address
    /// </summary>
    /// <param name="address">The wallet address to validate</param>
    /// <param name="cryptoCode">Optional cryptocurrency code (e.g., "btc", "eth") for specific validation</param>
    /// <returns>True if the address is valid, false otherwise</returns>
    public static bool IsValidWalletAddress(this string address, string? cryptoCode = null)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        // Check basic length constraints
        if (address.Length < MinAddressLength || address.Length > MaxAddressLength)
            return false;

        // If specific crypto code is provided, validate against its pattern
        if (!string.IsNullOrWhiteSpace(cryptoCode))
        {
            return ValidateSpecificCryptoAddress(address, cryptoCode);
        }

        // Generic validation (less strict) if no specific crypto code is provided
        return ValidateGenericCryptoAddress(address);
    }

    /// <summary>
    /// Validates a wallet address against a specific cryptocurrency's pattern
    /// </summary>
    private static bool ValidateSpecificCryptoAddress(string address, string cryptoCode)
    {
        var normalizedCryptoCode = cryptoCode.ToLowerInvariant();

        foreach (var (prefix, pattern) in CryptoPatterns)
        {
            if (normalizedCryptoCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return Regex.IsMatch(address, pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        // If no specific pattern is found, fall back to generic validation
        return ValidateGenericCryptoAddress(address);
    }

    /// <summary>
    /// Performs generic validation that applies to most cryptocurrency addresses
    /// </summary>
    private static bool ValidateGenericCryptoAddress(string address)
    {
        // Check for invalid characters (basic alphanumeric and common separators)
        if (!Regex.IsMatch(address, "^[a-zA-Z0-9_\\-.]{1,}$"))
            return false;

        // Check for suspicious patterns that might indicate an attack
        if (address.Contains("..") || address.Contains("//") || address.Contains("\\"))
            return false;

        return true;
    }

    /// <summary>
    /// Throws an ArgumentException if the wallet address is invalid
    /// </summary>
    /// <param name="address">The wallet address to validate</param>
    /// <param name="paramName">The name of the parameter being validated</param>
    /// <param name="cryptoCode">Optional cryptocurrency code for specific validation</param>
    /// <exception cref="ArgumentException">Thrown if the address is invalid</exception>
    public static void EnsureValidWalletAddress(this string address, string paramName, string? cryptoCode = null)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Wallet address cannot be null or empty.", paramName);

        if (address.Length < MinAddressLength || address.Length > MaxAddressLength)
            throw new ArgumentException(
                $"Wallet address must be between {MinAddressLength} and {MaxAddressLength} characters long.",
                paramName);

        if (!string.IsNullOrWhiteSpace(cryptoCode) && !ValidateSpecificCryptoAddress(address, cryptoCode))
            throw new ArgumentException(
                $"Invalid {cryptoCode.ToUpperInvariant()} wallet address format.",
                paramName);

        if (!ValidateGenericCryptoAddress(address))
            throw new ArgumentException("Invalid wallet address format.", paramName);
    }
}
