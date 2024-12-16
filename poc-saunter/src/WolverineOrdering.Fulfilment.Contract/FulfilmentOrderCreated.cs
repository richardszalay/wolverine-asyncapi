namespace WolverineOrdering.Fulfilment.Contract;

public record FulfilmentOrderCreated(
    Guid OrderId,
    string Provider
);
