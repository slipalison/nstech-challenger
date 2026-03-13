using MediatR;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;

namespace NsTech.Application.Features.Orders.CreateOrder;

/// <summary>
/// Solicitação para criar um item no pedido.
/// </summary>
/// <param name="ProductId">O ID único do produto.</param>
/// <param name="Quantity">A quantidade desejada (deve ser maior que 0).</param>
public record CreateOrderItemRequest(Guid ProductId, int Quantity);

/// <summary>
/// Comando para criar um novo pedido no sistema.
/// </summary>
/// <param name="CustomerId">Identificador único do cliente.</param>
/// <param name="Currency">Moeda do pedido (ex: BRL, USD).</param>
/// <param name="Items">Lista de itens que compõem o pedido.</param>
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
