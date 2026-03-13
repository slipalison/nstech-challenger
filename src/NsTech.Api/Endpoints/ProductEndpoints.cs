using MediatR;
using NsTech.Application.Features.Products.CreateProduct;
using NsTech.Application.Features.Products.GetProducts;

namespace NsTech.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/products")
            .WithTags("Products")
            .RequireAuthorization();

        products.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Cadastra um novo produto")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized);

        products.MapGet("/", ListProducts)
            .WithName("ListProducts")
            .WithSummary("Lista todos os produtos")
            .Produces<IEnumerable<ProductResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> CreateProduct(CreateProductCommand command, IMediator mediator)
    {
        var id = await mediator.Send(command);
        return Results.Created($"/products/{id}", new { id });
    }

    private static async Task<IResult> ListProducts(IMediator mediator)
    {
        var result = await mediator.Send(new GetProductsQuery());
        return Results.Ok(result);
    }
}
