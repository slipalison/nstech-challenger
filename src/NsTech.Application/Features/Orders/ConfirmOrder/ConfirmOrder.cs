using MediatR;
using NsTech.Domain.Interfaces;
using NsTech.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NsTech.Domain.Exceptions;

namespace NsTech.Application.Features.Orders.ConfirmOrder;

public record ConfirmOrderCommand(Guid OrderId) : IRequest<bool>;

public class ConfirmOrderHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ConfirmOrderCommand, bool>
{
    public async Task<bool> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new ResourceNotFoundException($"Pedido {request.OrderId} não encontrado.");

        if (order.Status == OrderStatus.Confirmed)
            return true; // Idempotente

        foreach (var item in order.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
                throw new KeyNotFoundException($"Produto {item.ProductId} não encontrado para reserva de estoque.");

            product.ReserveStock(item.Quantity);
            productRepository.Update(product);
        }

        order.Confirm();
        orderRepository.Update(order);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Em caso de concorrência, verificamos se o pedido já está no estado desejado
            var reloadedOrder = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (reloadedOrder?.Status == OrderStatus.Confirmed)
                return true; // Idempotência concorrente: outra instância já confirmou

            throw new InvalidOperationException("Conflito de concorrência ao confirmar pedido. Tente novamente.");
        }

        return true;
    }
}
