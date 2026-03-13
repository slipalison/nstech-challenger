using MediatR;
using NsTech.Application.Features.Orders.CancelOrder;
using NsTech.Application.Features.Orders.ConfirmOrder;
using NsTech.Application.Features.Orders.CreateOrder;
using NsTech.Application.Features.Orders.GetOrder;
using NsTech.Application.Features.Orders.ListOrders;

namespace NsTech.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/orders")
            .WithTags("Orders")
            .RequireAuthorization();

        orders.MapPost("/", CreateOrder)
            .WithName("CreateOrder")
            .WithSummary("Cria um novo pedido")
            .WithDescription("Processa a criação de um pedido para um cliente e lista de produtos.")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        orders.MapPost("/{id:guid}/confirm", ConfirmOrder)
            .WithName("ConfirmOrder")
            .WithSummary("Confirma um pedido")
            .WithDescription("Altera o status do pedido para 'Confirmed'.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);

        orders.MapPost("/{id:guid}/cancel", CancelOrder)
            .WithName("CancelOrder")
            .WithSummary("Cancela um pedido")
            .WithDescription("Altera o status do pedido para 'Cancelled'.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status401Unauthorized);

        orders.MapGet("/{id:guid}", GetOrder)
            .WithName("GetOrder")
            .WithSummary("Obtém detalhes de um pedido")
            .Produces<OrderResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        orders.MapGet("/", ListOrders)
            .WithName("ListOrders")
            .WithSummary("Lista pedidos com filtros")
            .Produces<ListOrdersResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> CreateOrder(CreateOrderCommand command, IMediator mediator)
    {
        var id = await mediator.Send(command);
        return Results.Created($"/orders/{id}", new { id });
    }

    private static async Task<IResult> ConfirmOrder(Guid id, IMediator mediator)
    {
        try
        {
            await mediator.Send(new ConfirmOrderCommand(id));
            return Results.NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("concorr�ncia"))
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CancelOrder(Guid id, IMediator mediator)
    {
        await mediator.Send(new CancelOrderCommand(id));
        return Results.NoContent();
    }

    private static async Task<IResult> GetOrder(Guid id, IMediator mediator)
    {
        var order = await mediator.Send(new GetOrderQuery(id));
        return order != null ? Results.Ok(order) : Results.NotFound();
    }

    private static async Task<IResult> ListOrders([AsParameters] ListOrdersQuery query, IMediator mediator)
    {
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }
}
