using PaymentGateway.Api.Dtos;

namespace PaymentGateway.Api.Services;

public class MockPaymentProcessor : IPaymentProcessor
{
    private static readonly HashSet<string> DeclinedCards = new()
    {
        "4000000000000002"
    };

    public ProcessorResult Process(PaymentRequest request)
    {
        var normalizedCard = new string(request.CardNumber.Where(char.IsDigit).ToArray());

        if (DeclinedCards.Contains(normalizedCard))
        {
            return new ProcessorResult(false, string.Empty, "Card declined by issuer.");
        }

        if (request.Amount > 10000m)
        {
            return new ProcessorResult(false, string.Empty, "Amount exceeds allowed limit.");
        }

        var authorizationCode = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
        return new ProcessorResult(true, authorizationCode, null);
    }
}
