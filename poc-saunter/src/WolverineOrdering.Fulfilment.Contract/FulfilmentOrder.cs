using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WolverineOrdering.Fulfilment.Contract;

public record FulfilmentOrder(
    string Id,
    FulfilmentOrderStatus Status,
    [property: MemberNotNullWhen(true, nameof(FulfilmentOrder.IsComplete))]
    string? ConfirmationId = null)
{
    [JsonIgnore]
    public bool IsComplete => Status == FulfilmentOrderStatus.Complete;
}

public enum FulfilmentOrderStatus
{
    Pending = 0,
    Complete = 1,
    Failed = 2
}

