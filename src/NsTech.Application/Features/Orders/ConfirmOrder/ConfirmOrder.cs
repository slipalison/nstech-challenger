using MediatR;
using NsTech.Domain.Interfaces;
using NsTech.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
            throw new KeyNotFoundException($"Pedido {request.OrderId} não encontrado.");

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
            // O desafio sugere retornar 409 Conflict em caso de controle otimista.
            // Vou relançar para ser tratado no middleware global ou na API.
            throw new InvalidOperationException("Conflito de concorrência ao reservar estoque. Tente novamente.");
        }

        return true;
    }
}
