using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Dtos;

public class PaymentRequest
{
    [Required]
    public string CardHolderName { get; set; } = string.Empty;

    [Required]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12)]
    public int ExpiryMonth { get; set; }

    [Range(2000, 2100)]
    public int ExpiryYear { get; set; }

    [Required]
    public string Cvv { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = "USD";
}
