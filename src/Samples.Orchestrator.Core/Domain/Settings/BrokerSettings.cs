namespace Samples.Orchestrator.Core.Domain.Settings;

public record BrokerSettings
{
    public required string Host { get; set; }
    public required string Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required Endpoints Endpoints { get; set; }
}

public record Endpoints
{
    public required string ConsumerGroup { get; set; }

    public required string Initial { get; set; }

    public required string PaymentSubmitted { get; set; }
    public required string PaymentAccepted { get; set; }
    public required string PaymentCancelled { get; set; }
    public required string PaymentDeadLetter { get; set; }
    public required string PaymentProcessing { get; set; }
    
    public required string ShippingSubmitted { get; set; }
    public required string ShippingAccepted { get; set; }
    public required string ShippingCancelled { get; set; }
    public required string ShippingDeadLetter { get; set; }
    public required string ShippingProcessing { get; set; }

    public required string Final { get; set; }
}