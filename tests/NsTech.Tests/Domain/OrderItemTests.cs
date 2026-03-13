using FluentAssertions;
using NsTech.Domain.Entities;

namespace NsTech.Tests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Constructor_With_Valid_Arguments_Should_Initialize()
    {
        var productId = Guid.NewGuid();
        var item = new OrderItem(productId, 100, 2);

        item.ProductId.Should().Be(productId);
        item.UnitPrice.Should().Be(100);
        item.Quantity.Should().Be(2);
        item.CalculateTotal().Should().Be(200);
    }

    [Fact]
    public void Constructor_With_Invalid_Quantity_Should_Throw()
    {
        var act = () => new OrderItem(Guid.NewGuid(), 100, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_With_Invalid_Price_Should_Throw()
    {
        var act = () => new OrderItem(Guid.NewGuid(), -1, 1);
        act.Should().Throw<ArgumentException>();
    }
}
