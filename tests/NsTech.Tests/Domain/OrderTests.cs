using FluentAssertions;
using NsTech.Domain.Entities;
using NsTech.Domain.Enums;

namespace NsTech.Tests.Domain;

public class OrderTests
{
    [Fact]
    public void Constructor_With_Invalid_Arguments_Should_Throw()
    {
        // Customer
        var act1 = () => new Order(Guid.NewGuid(), "", "BRL", [new OrderItem(Guid.NewGuid(), 100, 1)]);
        act1.Should().Throw<ArgumentException>();

        // Currency
        var act2 = () => new Order(Guid.NewGuid(), "cust-1", "", [new OrderItem(Guid.NewGuid(), 100, 1)]);
        act2.Should().Throw<ArgumentException>();

        // Items
        var act3 = () => new Order(Guid.NewGuid(), "cust-1", "BRL", []);
        act3.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateOrder_Should_Set_Status_To_Placed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var items = new List<OrderItem> { new(Guid.NewGuid(), 100, 1) };

        // Act
        var order = new Order(id, "customer-1", "BRL", items);

        // Assert
        order.Status.Should().Be(OrderStatus.Placed);
        order.Total.Should().Be(100);
        order.Items.Should().HaveCount(1);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Confirm_Should_Change_Status_To_Confirmed()
    {
        // Arrange
        var items = new List<OrderItem> { new(Guid.NewGuid(), 100, 1) };
        var order = new Order(Guid.NewGuid(), "customer-1", "BRL", items);

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_When_Already_Confirmed_Should_Be_Idempotent()
    {
        // Arrange
        var items = new List<OrderItem> { new(Guid.NewGuid(), 100, 1) };
        var order = new Order(Guid.NewGuid(), "customer-1", "BRL", items);
        order.Confirm();

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public void Cancel_When_Placed_Should_Change_Status_To_Canceled()
    {
        // Arrange
        var items = new List<OrderItem> { new(Guid.NewGuid(), 100, 1) };
        var order = new Order(Guid.NewGuid(), "customer-1", "BRL", items);

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact]
    public void Cancel_When_Confirmed_Should_Change_Status_To_Canceled()
    {
        // Arrange
        var items = new List<OrderItem> { new(Guid.NewGuid(), 100, 1) };
        var order = new Order(Guid.NewGuid(), "customer-1", "BRL", items);
        order.Confirm();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact]
    public void Cancel_When_Already_Canceled_Should_Be_Idempotent()
    {
        // Arrange
        var items = new List<OrderItem> { new(Guid.NewGuid(), 100, 1) };
        var order = new Order(Guid.NewGuid(), "customer-1", "BRL", items);
        order.Cancel();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Canceled);
    }

    [Fact]
    public void Confirm_When_Canceled_Should_Throw()
    {
        // Arrange
        var items = new List<OrderItem> { new(Guid.NewGuid(), 100, 1) };
        var order = new Order(Guid.NewGuid(), "customer-1", "BRL", items);
        order.Cancel();

        // Act
        var act = () => order.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
