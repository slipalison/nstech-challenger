using MediatR;
using NsTech.Domain.Interfaces;

namespace NsTech.Application.Features.Products.GetProducts;

public record ProductResponse(Guid Id, string Name, decimal UnitPrice, int AvailableQuantity);

public record GetProductsQuery : IRequest<IEnumerable<ProductResponse>>;

public class GetProductsHandler(IProductRepository productRepository) : IRequestHandler<GetProductsQuery, IEnumerable<ProductResponse>>
{
    public async Task<IEnumerable<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);
        return products.Select(p => new ProductResponse(p.Id, p.Name, p.UnitPrice, p.AvailableQuantity));
    }
}
