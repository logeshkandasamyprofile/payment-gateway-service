using PaymentGateway.Api.Dtos;

namespace PaymentGateway.Api.Services;

public record ValidationResult(bool IsValid, string? Error)
{
    public static ValidationResult Success() => new(true, null);

    public static ValidationResult Fail(string error) => new(false, error);
}

public interface ICardValidator
{
    ValidationResult Validate(PaymentRequest request);
}
