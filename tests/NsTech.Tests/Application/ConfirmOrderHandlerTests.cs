using FluentAssertions;
using Moq;
using NsTech.Application.Features.Orders.ConfirmOrder;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;
using NsTech.Domain.Enums;

namespace NsTech.Tests.Application;

public class ConfirmOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly ConfirmOrderHandler _handler;

    public ConfirmOrderHandlerTests()
    {
        _handler = new ConfirmOrderHandler(_orderRepoMock.Object, _productRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPlacedOrder_ShouldConfirmAndReserveStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Test Product", 100, 10);
        var item = new OrderItem(productId, 100, 2);
        var order = new Order(Guid.NewGuid(), "cust-1", "BRL", [item]);

        _orderRepoMock.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        product.AvailableQuantity.Should().Be(8);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAlreadyConfirmed_ShouldBeIdempotent()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), "cust-1", "BRL", [new OrderItem(Guid.NewGuid(), 100, 1)]);
        order.Confirm();

        _orderRepoMock.Setup(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _productRepoMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
