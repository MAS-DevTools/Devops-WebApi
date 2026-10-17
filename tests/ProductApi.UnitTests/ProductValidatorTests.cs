using ProductApi.Products;

namespace ProductApi.UnitTests;

public class ProductValidatorTests
{
    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        var errors = ProductValidator.Validate(new ProductRequest("Coffee", 2.50m));

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingName_ReturnsNameError(string? name)
    {
        var errors = ProductValidator.Validate(new ProductRequest(name, 2.50m));

        Assert.Equal(["Name is required."], errors["Name"]);
    }

    [Fact]
    public void Validate_NameTooLong_ReturnsNameError()
    {
        var name = new string('a', ProductValidator.NameMaxLength + 1);

        var errors = ProductValidator.Validate(new ProductRequest(name, 2.50m));

        Assert.True(errors.ContainsKey("Name"));
    }

    [Fact]
    public void Validate_NameAtMaxLength_ReturnsNoErrors()
    {
        var name = new string('a', ProductValidator.NameMaxLength);

        var errors = ProductValidator.Validate(new ProductRequest(name, 2.50m));

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_PriceNotPositive_ReturnsPriceError(decimal price)
    {
        var errors = ProductValidator.Validate(new ProductRequest("Coffee", price));

        Assert.Equal(["Price must be greater than 0."], errors["Price"]);
    }

    [Fact]
    public void Validate_PriceAboveMaximum_ReturnsPriceError()
    {
        var errors = ProductValidator.Validate(new ProductRequest("Coffee", ProductValidator.MaxPrice + 0.01m));

        Assert.True(errors.ContainsKey("Price"));
    }

    [Fact]
    public void Validate_NameAndPriceInvalid_ReturnsBothErrors()
    {
        var errors = ProductValidator.Validate(new ProductRequest("", 0));

        Assert.Equal(2, errors.Count);
    }
}
