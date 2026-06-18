using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace PaymentGateway.Api.Services;

public class EncryptionOptions
{
    public const string SectionName = "Encryption";

    public string Key { get; set; } = string.Empty;
}

public class AesCardEncryptionService : ICardEncryptionService
{
    private readonly byte[] _key;

    public AesCardEncryptionService(IOptions<EncryptionOptions> options)
    {
        var configuredKey = options.Value.Key;
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            throw new InvalidOperationException("Encryption key is not configured.");
        }

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var data = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        if (data.Length < iv.Length)
        {
            throw new CryptographicException("Cipher text is malformed.");
        }

        Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = decryptor.TransformFinalBlock(data, iv.Length, data.Length - iv.Length);

        return Encoding.UTF8.GetString(cipherBytes);
    }
}
