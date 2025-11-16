using System.Security.Cryptography;
using System.Text;

namespace NetCore_Learning.Share.Helper;

/// <summary>
/// Helper class chứa các utility methods dùng chung
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// Hash token bằng SHA256 và trả về Base64 string
    /// Dùng để tạo key cho Redis blacklist hoặc các mục đích khác
    /// </summary>
    /// <param name="token">Token cần hash</param>
    /// <returns>Base64 encoded hash string</returns>
    public static string HashToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Hash string bằng SHA256 và trả về Base64 string
    /// </summary>
    /// <param name="input">String cần hash</param>
    /// <returns>Base64 encoded hash string</returns>
    public static string HashString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Hash string bằng SHA256 và trả về hex string
    /// </summary>
    /// <param name="input">String cần hash</param>
    /// <returns>Hex encoded hash string</returns>
    public static string HashStringToHex(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}