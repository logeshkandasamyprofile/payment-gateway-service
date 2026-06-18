using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default);
}
