using FluentAssertions;
using Moq;
using NsTech.Application.Features.Orders.CancelOrder;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using NsTech.Domain.Enums;

namespace NsTech.Tests.Application;

public class CancelOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CancelOrderHandler _handler;

    public CancelOrderHandlerTests()
    {
        _handler = new CancelOrderHandler(_orderRepoMock.Object, _productRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_WhenConfirmed_ShouldCancelAndReleaseStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Test Product", 100, 8);
        var item = new OrderItem(productId, 100, 2);
        var order = new Order(Guid.NewGuid(), "cust-1", "BRL", [item]);
        order.Confirm();

        _orderRepoMock.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
        product.AvailableQuantity.Should().Be(10);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPlaced_ShouldCancelWithoutReleasingStock()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), "cust-1", "BRL", [new OrderItem(Guid.NewGuid(), 100, 1)]);

        _orderRepoMock.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Canceled);
        _productRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenConcurrencyOccursAndOrderIsCanceled_ShouldReturnTrue()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), "cust-1", "BRL", [new OrderItem(Guid.NewGuid(), 100, 1)]);

        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var canceledOrder = new Order(order.Id, "cust-1", "BRL", [new OrderItem(Guid.NewGuid(), 100, 1)]);
        canceledOrder.Cancel();

        _orderRepoMock.SetupSequence(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order)         // Inicial
            .ReturnsAsync(canceledOrder); // No catch

        // Act
        var result = await _handler.Handle(new CancelOrderCommand(order.Id), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }
}
