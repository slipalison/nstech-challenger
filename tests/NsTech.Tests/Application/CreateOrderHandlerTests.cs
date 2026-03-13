using FluentAssertions;
using Moq;
using NsTech.Application.Features.Orders.CreateOrder;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;

namespace NsTech.Tests.Application;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _handler = new CreateOrderHandler(_orderRepoMock.Object, _productRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateOrder()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Test Product", 100, 10);
        _productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var request = new CreateOrderCommand("cust-1", "BRL", 
            [new CreateOrderItemRequest(productId, 2)]);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _orderRepoMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInsufficientStock_ShouldThrow()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Test Product", 100, 1);
        _productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var request = new CreateOrderCommand("cust-1", "BRL", 
            [new CreateOrderItemRequest(productId, 2)]);

        // Act
        var act = () => _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
