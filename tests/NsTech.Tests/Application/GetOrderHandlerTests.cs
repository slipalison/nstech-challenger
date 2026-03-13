using FluentAssertions;
using Moq;
using NsTech.Application.Features.Orders.GetOrder;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;

namespace NsTech.Tests.Application;

public class GetOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly GetOrderHandler _handler;

    public GetOrderHandlerTests()
    {
        _handler = new GetOrderHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldReturnOrderResponse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var item = new OrderItem(Guid.NewGuid(), 100, 2);
        var order = new Order(orderId, "cust-1", "BRL", [item]);
        
        _orderRepoMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(new GetOrderQuery(orderId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.CustomerId.Should().Be("cust-1");
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepoMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(new GetOrderQuery(orderId), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
