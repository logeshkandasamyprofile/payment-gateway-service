using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> CreatePayment(
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _paymentService.ProcessPaymentAsync(request, cancellationToken);

        if (response.Status == PaymentStatus.Failure)
        {
            return UnprocessableEntity(response);
        }

        return CreatedAtAction(
            nameof(GetByReference),
            new { referenceNumber = response.ReferenceNumber },
            response);
    }

    [HttpGet("{referenceNumber}")]
    public async Task<ActionResult<Transaction>> GetByReference(
        string referenceNumber,
        CancellationToken cancellationToken)
    {
        var transaction = await _paymentService.GetByReferenceAsync(referenceNumber, cancellationToken);
        if (transaction is null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }
}
