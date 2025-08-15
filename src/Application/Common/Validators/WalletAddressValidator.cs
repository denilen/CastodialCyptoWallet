using System.Text.RegularExpressions;

namespace CryptoWallet.Application.Common.Validators;

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

    private static bool ValidateSpecificCryptoAddress(string address, string cryptoCode)
    {
        var normalizedCryptoCode = cryptoCode.ToLowerInvariant();

        // Find the pattern for the specified cryptocurrency
        foreach (var (prefix, pattern) in CryptoPatterns)
        {
            if (string.Equals(prefix, normalizedCryptoCode, StringComparison.OrdinalIgnoreCase))
            {
                return Regex.IsMatch(address, pattern, RegexOptions.Compiled);
            }
        }

        // If we don't have a specific pattern for this crypto, fall back to generic validation
        return ValidateGenericCryptoAddress(address);
    }

    private static bool ValidateGenericCryptoAddress(string address)
    {
        // Basic alphanumeric check with common separators
        if (!address.All(c => char.IsLetterOrDigit(c) || "_-:.".Contains(c)))
            return false;

        // Additional checks based on common patterns if needed
        if (address.StartsWith("0x") && address.Length == 42) // Likely Ethereum
        {
            // Check that the rest of the address is hexadecimal
            return address[2..].All(c => "0123456789abcdef".Contains(char.ToLower(c)));
        }

        // Add more specific checks for other common patterns if needed

        return true;
    }
}
