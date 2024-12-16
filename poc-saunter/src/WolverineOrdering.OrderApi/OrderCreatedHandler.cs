using Microsoft.AspNetCore.Mvc;
using Saunter.Attributes;
using WolverineOrdering.Fulfilment.Contract;
using WolverineOrdering.Order.Contract;

namespace WolverineOrdering.OrderApi;

// This would likely be a policy/saga that kicks off CreateFulfilmentOrder
public class OrderCreatedHandler
{
    [PublishOperation()]
    public static CreateFulfilmentOrder Handle(OrderCreated orderCreated)
    {
        return new CreateFulfilmentOrder(orderCreated.Id);
    }
}
