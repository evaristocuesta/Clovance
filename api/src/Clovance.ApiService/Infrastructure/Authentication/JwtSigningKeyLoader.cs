using System.Security.Cryptography;

namespace Clovance.ApiService.Infrastructure.Authentication;

public static class JwtSigningKeyLoader
{
    public static string LoadOrGenerate(string keyFilePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(keyFilePath)!);

        return File.Exists(keyFilePath)
            ? File.ReadAllText(keyFilePath)
            : GenerateAndPersist(keyFilePath);
    }

    private static string GenerateAndPersist(string keyFilePath)
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        File.WriteAllText(keyFilePath, key);
        return key;
    }
}
