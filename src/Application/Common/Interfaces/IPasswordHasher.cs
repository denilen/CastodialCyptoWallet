namespace CryptoWallet.Application.Common.Interfaces;

/// <summary>
/// Service for hashing and verifying passwords
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <returns>The hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    /// <param name="hashedPassword">The hashed password</param>
    /// <param name="providedPassword">The password provided for verification</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    bool VerifyPassword(string hashedPassword, string providedPassword);
}
