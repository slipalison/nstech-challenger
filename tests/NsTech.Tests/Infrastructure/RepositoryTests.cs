using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NsTech.Domain.Entities;
using NsTech.Infrastructure.Data;
using NsTech.Infrastructure.Repositories;

namespace NsTech.Tests.Infrastructure;

public class RepositoryTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ProductRepository_Add_And_Get_Should_Work()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new ProductRepository(context);
        var product = new Product(Guid.NewGuid(), "Test", 100, 10);

        // Act
        await repo.AddAsync(product, CancellationToken.None);
        await context.SaveChangesAsync();
        var result = await repo.GetByIdAsync(product.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task OrderRepository_ListAsync_Should_Filter_And_Paginate()
    {
        // Arrange
        var context = GetDbContext();
        var repo = new OrderRepository(context);
        
        var order1 = new Order(Guid.NewGuid(), "c1", "BRL", [new OrderItem(Guid.NewGuid(), 10, 1)]);
        var order2 = new Order(Guid.NewGuid(), "c2", "BRL", [new OrderItem(Guid.NewGuid(), 20, 1)]);
        
        await repo.AddAsync(order1, CancellationToken.None);
        await repo.AddAsync(order2, CancellationToken.None);
        await context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await repo.ListAsync("c1", null, null, null, 1, 10, CancellationToken.None);

        // Assert
        totalCount.Should().Be(1);
        items.First().CustomerId.Should().Be("c1");
    }
}
