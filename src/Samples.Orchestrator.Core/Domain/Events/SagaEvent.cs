using System.Text.Json.Nodes;
using MassTransit;

namespace Samples.Orchestrator.Core.Domain.Events;

public record SagaEvent : ISaga
{
    public Guid CorrelationId { get; set; }
    public required string CurrentState { get; set; }
    public int RetryCount { get; set; } = 0;
    public required JsonObject Payload { get; set; }
    public DateTime CreatedAt { get; set; }
}