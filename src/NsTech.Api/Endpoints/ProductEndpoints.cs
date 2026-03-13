using MediatR;
using NsTech.Application.Features.Products.CreateProduct;
using NsTech.Application.Features.Products.GetProducts;

namespace NsTech.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var products = app.MapGroup("/products").RequireAuthorization();

        products.MapPost("/", CreateProduct);
        products.MapGet("/", ListProducts);
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
