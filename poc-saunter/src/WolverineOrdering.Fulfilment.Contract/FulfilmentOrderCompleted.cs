namespace WolverineOrdering.Fulfilment.Contract;

public record FulfilmentOrderCompleted(
    Guid OrderId,
    FulfilmentOrderStatus Status
);
