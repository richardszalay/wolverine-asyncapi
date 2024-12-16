using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Wolverine;
using Wolverine.Http;
using WolverineOrdering.Order.Contract;

namespace WolverineOrdering.OrderApi;

public record CreateOrder();

public record CreateOrderResponse(Guid Id);

public class CreateOrderHandler
{
    [WolverinePost("/orders")]
    [ProducesResponseType(201, Type = typeof(CreateOrderResponse))]
    public static (CreateOrderResponse, OrderCreated) Handle(CreateOrder command)
    {
        // TBD Save to marten

        var orderId = Guid.NewGuid();

        Activity.Current?.AddTag("order.id", orderId.ToString());

        return (
            new CreateOrderResponse(orderId),
            new OrderCreated(orderId)
        );
    }
}