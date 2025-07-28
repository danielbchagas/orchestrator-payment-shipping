using Samples.Orchestrator.Core.Domain.Settings;

namespace Samples.Orchestrator.Core.Infrastructure.Factories;

public interface IBrokerSettingsFactory
{
    BrokerSettings Create(IConfiguration configuration);
}