using Samples.Orchestrator.Core.Infrastructure.Factories;

namespace Samples.Orchestrator.Core.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBrokerSettingsFactory, BrokerSettingsFactory>();

        return services;
    }
}