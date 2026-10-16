using SoftwareDeploymentProject.Models;
namespace SoftwareDeploymentProject.Tests;

public class ShopRulesTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(21)]
    [InlineData(int.MaxValue)]
    public void RejectsInvalidQuantities(int quantity) => Assert.Throws<ArgumentException>(() => ShopRules.ValidateQuantity(quantity));
    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void AcceptsQuantityBoundaries(int quantity) => ShopRules.ValidateQuantity(quantity);
    [Fact]
    public void TotalsUseIntegerCents()
    {
        var cart = new Cart([new(new(1, "Test", "Test", "Test", 199, "tee"), 3), new(new(2, "Other", "Test", "Test", 850, "tote"), 2)]);
        Assert.Equal(5, cart.ItemCount); Assert.Equal(2297, cart.TotalCents);
    }
    [Fact] public void EmptyCartHasZeroTotals() { var cart = new Cart([]); Assert.Equal(0, cart.ItemCount); Assert.Equal(0, cart.TotalCents); }
    [Fact] public void CurrencyUsesAustralianDollars() => Assert.Equal("$12.99", ShopRules.Money(1299));
}
