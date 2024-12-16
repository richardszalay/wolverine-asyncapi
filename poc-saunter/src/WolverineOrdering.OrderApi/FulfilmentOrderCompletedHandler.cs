using WolverineOrdering.Fulfilment.Contract;
using WolverineOrdering.Order.Contract;

namespace WolverineOrdering.OrderApi;

public class FulfilmentOrderCompletedHandler
{
    public static OrderCompleted Handle(FulfilmentOrderCompleted fulfilmentCompleted)
    {
        return new OrderCompleted(fulfilmentCompleted.OrderId, fulfilmentCompleted.Status switch
        {
            FulfilmentOrderStatus.Complete => OrderStatus.Complete,
            _ => OrderStatus.Failed
        });
    }
}
