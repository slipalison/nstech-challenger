using FluentAssertions;
using Moq;
using NsTech.Application.Features.Products.CreateProduct;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;

namespace NsTech.Tests.Application;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _handler = new CreateProductHandler(_productRepoMock.Object, _uowMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateProduct()
    {
        // Arrange
        var command = new CreateProductCommand(Guid.NewGuid(), "New Product", 50, 100);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(command.Id);
        _productRepoMock.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
