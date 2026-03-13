using FluentAssertions;
using Moq;
using NsTech.Application.Features.Orders.ListOrders;
using NsTech.Domain.Entities;
using NsTech.Domain.Enums;
using NsTech.Domain.Interfaces;

namespace NsTech.Tests.Application;

public class ListOrdersHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly ListOrdersHandler _handler;

    public ListOrdersHandlerTests()
    {
        _handler = new ListOrdersHandler(_orderRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WithFilters_ShouldReturnPagedResponse()
    {
        // Arrange
        var orders = new List<Order>
        {
            new(Guid.NewGuid(), "cust-1", "BRL", [new OrderItem(Guid.NewGuid(), 100, 1)]),
            new(Guid.NewGuid(), "cust-1", "BRL", [new OrderItem(Guid.NewGuid(), 200, 2)])
        };

        _orderRepoMock.Setup(x => x.ListAsync(
            It.IsAny<string>(), It.IsAny<OrderStatus?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders, 2));

        var query = new ListOrdersQuery(CustomerId: "cust-1");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
