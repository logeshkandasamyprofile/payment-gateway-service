using PaymentGateway.Api.Dtos;
using PaymentGateway.Api.Services;
using Xunit;

namespace PaymentGateway.Tests;

public class CardValidatorTests
{
    private readonly CardValidator _validator = new(TimeProvider.System);

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
    public void Validate_ValidCard_Succeeds()
    {
        var result = _validator.Validate(ValidRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_FailsLuhn()
    {
        var request = ValidRequest();
        request.CardNumber = "4242424242424241";
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ExpiredCard_Fails()
    {
        var request = ValidRequest();
        request.ExpiryYear = 2000;
        request.ExpiryMonth = 1;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("12a")]
    public void Validate_InvalidCvv_Fails(string cvv)
    {
        var request = ValidRequest();
        request.Cvv = cvv;
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
    }
}
