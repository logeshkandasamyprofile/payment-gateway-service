using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentGateway.Api.Data;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
using Xunit;

namespace PaymentGateway.Tests;

public class PaymentServiceTests
{
    private static PaymentService CreateService(out PaymentDbContext dbContext)
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        dbContext = new PaymentDbContext(options);

        var encryption = new AesCardEncryptionService(
            Options.Create(new EncryptionOptions { Key = "unit-test-key" }));

        return new PaymentService(
            dbContext,
            new CardValidator(TimeProvider.System),
            encryption,
            new MockPaymentProcessor(),
            TimeProvider.System);
    }

    private static PaymentRequest ValidRequest() => new()
    {
        CardHolderName = "Jane Doe",
        CardNumber = "4242 4242 4242 4242",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Cvv = "123",
        Amount = 49.99m,
        Currency = "USD"
    };

    [Fact]
    public async Task ProcessPayment_Success_PersistsAndReturnsReference()
    {
        var service = CreateService(out var dbContext);

        var response = await service.ProcessPaymentAsync(ValidRequest());

        Assert.Equal(PaymentStatus.Success, response.Status);
        Assert.StartsWith("PG-", response.ReferenceNumber);
        Assert.StartsWith("TXN-", response.TransactionId);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));

        var stored = await dbContext.Transactions.SingleAsync();
        Assert.Equal(response.ReferenceNumber, stored.ReferenceNumber);
        Assert.Equal("************4242", stored.MaskedCardNumber);
        Assert.DoesNotContain("4242424242424242", stored.EncryptedCardData);
    }

    [Fact]
    public async Task ProcessPayment_DeclinedCard_MarksFailure()
    {
        var service = CreateService(out _);
        var request = ValidRequest();
        request.CardNumber = "4000000000000002";

        var response = await service.ProcessPaymentAsync(request);

        Assert.Equal(PaymentStatus.Failure, response.Status);
    }

    [Fact]
    public async Task ProcessPayment_InvalidCard_MarksFailure()
    {
        var service = CreateService(out _);
        var request = ValidRequest();
        request.CardNumber = "1234567890123456";

        var response = await service.ProcessPaymentAsync(request);

        Assert.Equal(PaymentStatus.Failure, response.Status);
    }

    [Fact]
    public async Task GetByReference_ReturnsStoredTransaction()
    {
        var service = CreateService(out _);
        var response = await service.ProcessPaymentAsync(ValidRequest());

        var found = await service.GetByReferenceAsync(response.ReferenceNumber);

        Assert.NotNull(found);
        Assert.Equal(response.TransactionId, found!.TransactionId);
    }
}
