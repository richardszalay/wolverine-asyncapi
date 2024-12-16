using Wolverine;
using WolverineOrdering.Fulfilment.Contract;

namespace WolverineOrdering.FulfilmentApi;

public class CreateFulfilmentOrderHandler
{
    public static OutgoingMessages Handle(CreateFulfilmentOrder createOrder)
    {
        return new OutgoingMessages()
        {
            new FulfilmentOrderCreated(createOrder.OrderId, "Temp"),

            new FulfilmentOrderCompleted(createOrder.OrderId, FulfilmentOrderStatus.Complete)
                .DelayedFor(TimeSpan.FromSeconds(5))
        };
    }
}