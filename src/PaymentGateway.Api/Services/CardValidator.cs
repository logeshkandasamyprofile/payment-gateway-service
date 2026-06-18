using PaymentGateway.Api.Dtos;

namespace PaymentGateway.Api.Services;

public class CardValidator : ICardValidator
{
    private readonly TimeProvider _timeProvider;

    public CardValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public ValidationResult Validate(PaymentRequest request)
    {
        var digits = new string(request.CardNumber.Where(char.IsDigit).ToArray());

        if (digits.Length is < 13 or > 19)
        {
            return ValidationResult.Fail("Card number length is invalid.");
        }

        if (!PassesLuhn(digits))
        {
            return ValidationResult.Fail("Card number failed checksum validation.");
        }

        if (request.ExpiryMonth is < 1 or > 12)
        {
            return ValidationResult.Fail("Expiry month is invalid.");
        }

        var now = _timeProvider.GetUtcNow();
        var lastDayOfExpiry = new DateTimeOffset(
            request.ExpiryYear,
            request.ExpiryMonth,
            DateTime.DaysInMonth(request.ExpiryYear, request.ExpiryMonth),
            23, 59, 59,
            TimeSpan.Zero);

        if (lastDayOfExpiry < now)
        {
            return ValidationResult.Fail("Card has expired.");
        }

        var cvvDigits = new string(request.Cvv.Where(char.IsDigit).ToArray());
        if (cvvDigits.Length is < 3 or > 4 || cvvDigits.Length != request.Cvv.Length)
        {
            return ValidationResult.Fail("CVV is invalid.");
        }

        if (request.Amount <= 0)
        {
            return ValidationResult.Fail("Amount must be greater than zero.");
        }

        return ValidationResult.Success();
    }

    private static bool PassesLuhn(string digits)
    {
        var sum = 0;
        var alternate = false;

        for (var i = digits.Length - 1; i >= 0; i--)
        {
            var n = digits[i] - '0';
            if (alternate)
            {
                n *= 2;
                if (n > 9)
                {
                    n -= 9;
                }
            }

            sum += n;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }
}
