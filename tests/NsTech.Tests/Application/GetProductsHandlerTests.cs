using FluentAssertions;
using Moq;
using NsTech.Application.Features.Products.GetProducts;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;

namespace NsTech.Tests.Application;

public class GetProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _handler = new GetProductsHandler(_productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new(Guid.NewGuid(), "P1", 10, 10),
            new(Guid.NewGuid(), "P2", 20, 20)
        };
        _productRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("P1");
    }
}
