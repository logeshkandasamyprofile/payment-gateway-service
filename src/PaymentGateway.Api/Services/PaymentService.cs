using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Api.Data;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentDbContext _dbContext;
    private readonly ICardValidator _validator;
    private readonly ICardEncryptionService _encryption;
    private readonly IPaymentProcessor _processor;
    private readonly TimeProvider _timeProvider;

    public PaymentService(
        PaymentDbContext dbContext,
        ICardValidator validator,
        ICardEncryptionService encryption,
        IPaymentProcessor processor,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _validator = validator;
        _encryption = encryption;
        _processor = processor;
        _timeProvider = timeProvider;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var validation = _validator.Validate(request);

        var normalizedCard = new string(request.CardNumber.Where(char.IsDigit).ToArray());

        var transaction = new Transaction
        {
            TransactionId = GenerateTransactionId(),
            Token = GenerateToken(),
            ReferenceNumber = GenerateReferenceNumber(),
            Amount = request.Amount,
            Currency = request.Currency,
            CardHolderName = request.CardHolderName,
            MaskedCardNumber = MaskCard(normalizedCard),
            EncryptedCardData = EncryptCardData(request, normalizedCard),
            CreatedAt = _timeProvider.GetUtcNow()
        };

        if (!validation.IsValid)
        {
            transaction.Status = PaymentStatus.Failure;
            transaction.FailureReason = validation.Error;
        }
        else
        {
            var processorResult = _processor.Process(request);
            if (processorResult.Approved)
            {
                transaction.Status = PaymentStatus.Success;
            }
            else
            {
                transaction.Status = PaymentStatus.Failure;
                transaction.FailureReason = processorResult.DeclineReason;
            }
        }

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PaymentResponse
        {
            ReferenceNumber = transaction.ReferenceNumber,
            TransactionId = transaction.TransactionId,
            Token = transaction.Token,
            Status = transaction.Status,
            Message = transaction.Status == PaymentStatus.Success
                ? "Payment processed successfully."
                : transaction.FailureReason ?? "Payment failed."
        };
    }

    public async Task<Transaction?> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.ReferenceNumber == referenceNumber, cancellationToken);
    }

    private string EncryptCardData(PaymentRequest request, string normalizedCard)
    {
        var payload = JsonSerializer.Serialize(new
        {
            request.CardHolderName,
            CardNumber = normalizedCard,
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Cvv
        });

        return _encryption.Encrypt(payload);
    }

    private static string MaskCard(string normalizedCard)
    {
        if (normalizedCard.Length <= 4)
        {
            return new string('*', normalizedCard.Length);
        }

        var last4 = normalizedCard[^4..];
        return new string('*', normalizedCard.Length - 4) + last4;
    }

    private static string GenerateTransactionId() => $"TXN-{Guid.NewGuid():N}";

    private static string GenerateToken() => Guid.NewGuid().ToString("N");

    private string GenerateReferenceNumber()
    {
        var timestamp = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"PG-{timestamp}-{suffix}";
    }
}
