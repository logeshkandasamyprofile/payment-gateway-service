using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Models;
using Xunit;

namespace PaymentGateway.Tests;

public class PaymentsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PaymentsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private static PaymentRequest ValidRequest() => new()
    {
        CardHolderName = "Jane Doe",
        CardNumber = "4242424242424242",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Cvv = "123",
        Amount = 49.99m,
        Currency = "USD"
    };

    [Fact]
    public async Task PostPayment_Success_ReturnsReferenceAndIsRetrievable()
    {
        var client = _factory.CreateClient();

        var post = await client.PostAsJsonAsync("/api/payments", ValidRequest());
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var payment = await post.Content.ReadFromJsonAsync<PaymentResponse>();
        Assert.NotNull(payment);
        Assert.Equal(PaymentStatus.Success, payment!.Status);
        Assert.StartsWith("PG-", payment.ReferenceNumber);

        var get = await client.GetAsync($"/api/payments/{payment.ReferenceNumber}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task PostPayment_DeclinedCard_ReturnsUnprocessable()
    {
        var client = _factory.CreateClient();
        var request = ValidRequest();
        request.CardNumber = "4000000000000002";

        var post = await client.PostAsJsonAsync("/api/payments", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, post.StatusCode);
    }

    [Fact]
    public async Task GetPayment_UnknownReference_ReturnsNotFound()
    {
        var client = _factory.CreateClient();

        var get = await client.GetAsync("/api/payments/PG-does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
