using MediatR;
using NsTech.Domain.Entities;
using NsTech.Domain.Interfaces;

namespace NsTech.Application.Features.Products.CreateProduct;

/// <summary>
/// Comando para cadastrar um novo produto.
/// </summary>
/// <param name="Id">ID único do produto (Guid).</param>
/// <param name="Name">Nome do produto.</param>
/// <param name="UnitPrice">Preço unitário (deve ser maior que 0).</param>
/// <param name="AvailableQuantity">Quantidade disponível em estoque.</param>
public record CreateProductCommand(Guid Id, string Name, decimal UnitPrice, int AvailableQuantity) : IRequest<Guid>;

public class CreateProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork) 
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Id, request.Name, request.UnitPrice, request.AvailableQuantity);
        
        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return product.Id;
    }
}
