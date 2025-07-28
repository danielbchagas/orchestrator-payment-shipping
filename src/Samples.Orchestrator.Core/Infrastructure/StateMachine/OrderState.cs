using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using MassTransit;
using Samples.Orchestrator.Core.Domain.Events;

namespace Samples.Orchestrator.Core.Infrastructure.StateMachine;

public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public required string CurrentState { get; set; }
    public int RetryCount { get; set; } = 0;
    public required JsonObject Payload { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Initialize<T>(T message) where T : SagaEvent
    {
        CorrelationId = message.CorrelationId;
        CurrentState = message.CurrentState;
        Payload = message.Payload;
        CreatedAt = DateTime.UtcNow;
    }
}