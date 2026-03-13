using MediatR;
using NsTech.Domain.Interfaces;
using NsTech.Domain.Enums;

namespace NsTech.Application.Features.Orders.GetOrder;

public record OrderItemResponse(Guid ProductId, decimal UnitPrice, int Quantity);
public record OrderResponse(Guid Id, string CustomerId, OrderStatus Status, string Currency, decimal Total, DateTime CreatedAt, List<OrderItemResponse> Items);

public record GetOrderQuery(Guid Id) : IRequest<OrderResponse?>;

public class GetOrderHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrderQuery, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order == null) return null;

        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Status,
            order.Currency,
            order.Total,
            order.CreatedAt,
            order.Items.Select(i => new OrderItemResponse(i.ProductId, i.UnitPrice, i.Quantity)).ToList()
        );
    }
}
