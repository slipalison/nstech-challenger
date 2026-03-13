using MediatR;
using NsTech.Domain.Interfaces;
using NsTech.Domain.Enums;

namespace NsTech.Application.Features.Orders.CancelOrder;

public record CancelOrderCommand(Guid OrderId) : IRequest<bool>;

public class CancelOrderHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand, bool>
{
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new KeyNotFoundException($"Pedido {request.OrderId} não encontrado.");

        if (order.Status == OrderStatus.Canceled)
            return true; // Idempotente

        var wasConfirmed = order.Status == OrderStatus.Confirmed;

        order.Cancel();
        orderRepository.Update(order);

        if (wasConfirmed)
        {
            foreach (var item in order.Items)
            {
                var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
                if (product != null)
                {
                    product.ReleaseStock(item.Quantity);
                    productRepository.Update(product);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
