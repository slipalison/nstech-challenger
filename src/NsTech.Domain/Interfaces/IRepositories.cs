using NsTech.Domain.Entities;
using NsTech.Domain.Enums;

namespace NsTech.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Product product, CancellationToken cancellationToken);
    void Update(Product product);
    void Delete(Product product);
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(IEnumerable<Order> Items, int TotalCount)> ListAsync(
        string? customerId, 
        OrderStatus? status, 
        DateTime? from, 
        DateTime? to, 
        int page, 
        int pageSize, 
        CancellationToken cancellationToken);
    Task AddAsync(Order order, CancellationToken cancellationToken);
    void Update(Order order);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
