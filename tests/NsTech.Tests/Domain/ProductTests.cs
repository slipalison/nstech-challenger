using FluentAssertions;
using NsTech.Domain.Entities;

namespace NsTech.Tests.Domain;

public class ProductTests
{
    [Fact]
    public void Constructor_With_Invalid_Arguments_Should_Throw()
    {
        // Name
        var act1 = () => new Product(Guid.NewGuid(), "", 10, 10);
        act1.Should().Throw<ArgumentException>();

        // Price
        var act2 = () => new Product(Guid.NewGuid(), "Test", -1, 10);
        act2.Should().Throw<ArgumentException>().WithMessage("Preço deve ser maior que zero.");

        var actZeroPrice = () => new Product(Guid.NewGuid(), "Test", 0, 10);
        actZeroPrice.Should().Throw<ArgumentException>().WithMessage("Preço deve ser maior que zero.");

        // Stock
        var act3 = () => new Product(Guid.NewGuid(), "Test", 10, -1);
        act3.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateName_Should_Work_Or_Throw()
    {
        var product = new Product(Guid.NewGuid(), "Old", 10, 10);
        product.UpdateName("New");
        product.Name.Should().Be("New");

        var act = () => product.UpdateName("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdatePrice_Should_Work_Or_Throw()
    {
        var product = new Product(Guid.NewGuid(), "Test", 10, 10);
        product.UpdatePrice(20);
        product.UnitPrice.Should().Be(20);

        var act = () => product.UpdatePrice(-1);
        act.Should().Throw<ArgumentException>().WithMessage("Preço deve ser maior que zero.");

        var actZero = () => product.UpdatePrice(0);
        actZero.Should().Throw<ArgumentException>().WithMessage("Preço deve ser maior que zero.");
    }

    [Fact]
    public void UpdateStock_Should_Work_Or_Throw()
    {
        var product = new Product(Guid.NewGuid(), "Test", 10, 10);
        product.UpdateStock(50);
        product.AvailableQuantity.Should().Be(50);

        var act = () => product.UpdateStock(-1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReserveStock_Should_Decrease_AvailableQuantity()
    {
        // Arrange
        var product = new Product(Guid.NewGuid(), "Test Product", 100, 10);

        // Act
        product.ReserveStock(3);

        // Assert
        product.AvailableQuantity.Should().Be(7);
    }

    [Fact]
    public void ReserveStock_When_Insufficient_Should_Throw()
    {
        // Arrange
        var product = new Product(Guid.NewGuid(), "Test Product", 100, 2);

        // Act
        var act = () => product.ReserveStock(3);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReserveStock_With_Invalid_Quantity_Should_Throw()
    {
        var product = new Product(Guid.NewGuid(), "Test Product", 100, 10);
        var act = () => product.ReserveStock(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReleaseStock_Should_Increase_AvailableQuantity()
    {
        // Arrange
        var product = new Product(Guid.NewGuid(), "Test Product", 100, 10);

        // Act
        product.ReleaseStock(3);

        // Assert
        product.AvailableQuantity.Should().Be(13);
    }

    [Fact]
    public void ReleaseStock_With_Invalid_Quantity_Should_Throw()
    {
        var product = new Product(Guid.NewGuid(), "Test Product", 100, 10);
        var act = () => product.ReleaseStock(0);
        act.Should().Throw<ArgumentException>();
    }
}
