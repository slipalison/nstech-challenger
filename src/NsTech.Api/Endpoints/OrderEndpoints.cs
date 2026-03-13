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
        var orders = app.MapGroup("/orders").RequireAuthorization();

        orders.MapPost("/", async (CreateOrderCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Created($"/orders/{id}", new { id });
        });
        orders.MapPost("/{id:guid}/confirm", ConfirmOrder);
        orders.MapPost("/{id:guid}/cancel", CancelOrder);
        orders.MapGet("/{id:guid}", GetOrder);
        orders.MapGet("/", ListOrders);
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
