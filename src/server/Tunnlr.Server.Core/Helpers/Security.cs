using System.Security.Cryptography;

namespace Tunnlr.Server.Core.Helpers;

public static class Security
{
    /// <summary>
    /// Generates a secure byte array
    /// </summary>
    /// <returns></returns>
    public static byte[] GenerateSecureKey()
    {
        using var rng = RandomNumberGenerator.Create();
        
        var bytes = new byte[64];
        rng.GetBytes(bytes);

        return bytes;
    }
}