using FluentAssertions;
using Moq;
using NsTech.Application.Features.Orders.ConfirmOrder;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using NsTech.Domain.Enums;
using NsTech.Domain.Exceptions;

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
    public async Task Handle_WhenOrderDoesNotExist_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepoMock.Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var act = () => _handler.Handle(new ConfirmOrderCommand(orderId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>()
            .WithMessage($"Pedido {orderId} não encontrado.");
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

    [Fact]
    public async Task Handle_WhenConcurrencyOccursAndOrderIsConfirmed_ShouldReturnTrue()
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

        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Simula que ao recarregar, o pedido já está confirmado por outra instância
        var confirmedOrder = new Order(order.Id, "cust-1", "BRL", [item]);
        confirmedOrder.Confirm();
        _orderRepoMock.SetupSequence(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order)         // Primeira chamada no Handle
            .ReturnsAsync(confirmedOrder); // Chamada no catch do DbUpdateConcurrencyException

        // Act
        var result = await _handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _orderRepoMock.Verify(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WhenConcurrencyOccursAndOrderNotConfirmed_ShouldThrow()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product(productId, "Test Product", 100, 10);
        var item = new OrderItem(productId, 100, 2);
        var order = new Order(Guid.NewGuid(), "cust-1", "BRL", [item]);

        // Simula que ao recarregar, o pedido continua como PLACED
        // Nota: O objeto order deve ter Status == Placed
        var sameOrder = new Order(order.Id, "cust-1", "BRL", [item]);
        
        _orderRepoMock.SetupSequence(x => x.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order)      // Chamada 1: Get inicial no Handle
            .ReturnsAsync(sameOrder);  // Chamada 2: Get no catch

        _productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        var act = () => _handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Conflito de concorrência ao confirmar pedido. Tente novamente.");
    }
}
