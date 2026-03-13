using MediatR;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;

namespace NsTech.Application.Features.Orders.CreateOrder;

public record CreateOrderItemRequest(Guid ProductId, int Quantity);
public record CreateOrderCommand(string CustomerId, string Currency, List<CreateOrderItemRequest> Items) : IRequest<Guid>;

public class CreateOrderHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderItems = new List<OrderItem>();

        foreach (var itemRequest in request.Items)
        {
            var product = await productRepository.GetByIdAsync(itemRequest.ProductId, cancellationToken);
            if (product == null)
                throw new KeyNotFoundException($"Produto {itemRequest.ProductId} n�o encontrado.");

            if (product.AvailableQuantity < itemRequest.Quantity)
                throw new InvalidOperationException($"Estoque insuficiente para o produto {product.Id}.");

            var orderItem = new OrderItem(product.Id, product.UnitPrice, itemRequest.Quantity);
            orderItems.Add(orderItem);
        }

        var order = new Order(Guid.NewGuid(), request.CustomerId, request.Currency, orderItems);

        await orderRepository.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
