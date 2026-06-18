using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Dtos;

public class PaymentResponse
{
    public string ReferenceNumber { get; set; } = string.Empty;

    public string TransactionId { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; }

    public string Message { get; set; } = string.Empty;
}
