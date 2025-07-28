using Samples.Orchestrator.Core.Domain.Settings;

namespace Samples.Orchestrator.Core.Infrastructure.Factories;

public class BrokerSettingsFactory : IBrokerSettingsFactory
{
    private const string SectionName = "Broker";

    public BrokerSettings Create(IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(SectionName)
            .Get<BrokerSettings>();

        Validate(settings);

        return settings!;
    }

    private static void Validate(BrokerSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Host);
        ArgumentException.ThrowIfNullOrEmpty(settings.Port);
        ArgumentException.ThrowIfNullOrEmpty(settings.Username);
        ArgumentException.ThrowIfNullOrEmpty(settings.Password);
        
        ArgumentNullException.ThrowIfNull(settings.Endpoints);

        ArgumentNullException.ThrowIfNull(settings.Endpoints.ConsumerGroup);

        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentSubmitted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentAccepted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentCancelled);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentDeadLetter);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingSubmitted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingAccepted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingCancelled);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingDeadLetter);
    }
}