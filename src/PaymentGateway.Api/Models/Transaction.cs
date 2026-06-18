namespace PaymentGateway.Api.Models;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string TransactionId { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public string ReferenceNumber { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string CardHolderName { get; set; } = string.Empty;

    public string MaskedCardNumber { get; set; } = string.Empty;

    public string EncryptedCardData { get; set; } = string.Empty;

    public string? FailureReason { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
