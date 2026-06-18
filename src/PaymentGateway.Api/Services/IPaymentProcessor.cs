using PaymentGateway.Api.Dtos;

namespace PaymentGateway.Api.Services;

public record ProcessorResult(bool Approved, string AuthorizationCode, string? DeclineReason);

public interface IPaymentProcessor
{
    ProcessorResult Process(PaymentRequest request);
}
