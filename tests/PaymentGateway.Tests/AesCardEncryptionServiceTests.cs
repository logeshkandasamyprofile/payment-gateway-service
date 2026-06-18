using Microsoft.Extensions.Options;
using PaymentGateway.Api.Services;
using Xunit;

namespace PaymentGateway.Tests;

public class AesCardEncryptionServiceTests
{
    private static AesCardEncryptionService CreateService() =>
        new(Options.Create(new EncryptionOptions { Key = "unit-test-key" }));

    [Fact]
    public void EncryptThenDecrypt_RoundTrips()
    {
        var service = CreateService();
        const string plain = "4242424242424242";

        var cipher = service.Encrypt(plain);
        var decrypted = service.Decrypt(cipher);

        Assert.NotEqual(plain, cipher);
        Assert.Equal(plain, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCipherEachCall()
    {
        var service = CreateService();
        var first = service.Encrypt("secret");
        var second = service.Encrypt("secret");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Constructor_ThrowsWhenKeyMissing()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new AesCardEncryptionService(Options.Create(new EncryptionOptions { Key = "" })));
    }
}
