namespace PaymentGateway.Api.Services;

public interface ICardEncryptionService
{
    string Encrypt(string plainText);

    string Decrypt(string cipherText);
}
