using MediatR;
using NsTech.Domain.Interfaces;
using NsTech.Application.Features.Orders.GetOrder;
using NsTech.Domain.Enums;

namespace NsTech.Application.Features.Orders.ListOrders;

public record ListOrdersResponse(IEnumerable<OrderResponse> Items, int TotalCount, int Page, int PageSize);

public record ListOrdersQuery(
    string? CustomerId = null,
    OrderStatus? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 10) : IRequest<ListOrdersResponse>;

public class ListOrdersHandler(IOrderRepository orderRepository) : IRequestHandler<ListOrdersQuery, ListOrdersResponse>
{
    public async Task<ListOrdersResponse> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await orderRepository.ListAsync(
            request.CustomerId,
            request.Status,
            request.From,
            request.To,
            request.Page,
            request.PageSize,
            cancellationToken);

        var orderResponses = items.Select(order => new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Status,
            order.Currency,
            order.Total,
            order.CreatedAt,
            order.Items.Select(i => new OrderItemResponse(i.ProductId, i.UnitPrice, i.Quantity)).ToList()
        ));

        return new ListOrdersResponse(orderResponses, totalCount, request.Page, request.PageSize);
    }
}
