using Microsoft.EntityFrameworkCore;
using NsTech.Domain.Entities;
using NsTech.Domain.Enums;
using NsTech.Domain.Interfaces;
using NsTech.Infrastructure.Data;

namespace NsTech.Infrastructure.Repositories;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken)
        => await context.Products.ToListAsync(cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
        => await context.Products.AddAsync(product, cancellationToken);

    public void Update(Product product)
        => context.Products.Update(product);

    public void Delete(Product product)
        => context.Products.Remove(product);
}

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IEnumerable<Order> Items, int TotalCount)> ListAsync(
        string? customerId, 
        OrderStatus? status, 
        DateTime? from, 
        DateTime? to, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken)
    {
        var query = context.Orders.Include(x => x.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(customerId))
            query = query.Where(x => x.CustomerId == customerId);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
        => await context.Orders.AddAsync(order, cancellationToken);

    public void Update(Order order)
        => context.Orders.Update(order);
}
