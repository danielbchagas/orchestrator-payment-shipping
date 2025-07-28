using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Samples.Orchestrator.Core.Domain.Events.Final;
using Samples.Orchestrator.Core.Domain.Events.Start;
using Samples.Orchestrator.Core.Domain.Settings;
using Samples.Orchestrator.Core.Infrastructure.Database;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;
using System.Reflection;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Infrastructure.Extensions;

/*
 * https://masstransit.io/
 * https://masstransit.io/documentation/configuration/transports/rabbitmq
 * https://masstransit.io/documentation/transports/rabbitmq
 */

public static class MasstransitExtensions
{
    public static IServiceCollection AddMasstransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DbContext, OrderStateDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options =>
            {
                options.EnableRetryOnFailure();
            });
        });

        var kafkaSettings = BuildKafkaSettings(configuration);

        services.AddMassTransit(cfg =>
        {
            cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            
                    r.AddDbContext<SagaDbContext, OrderStateDbContext>((provider,builder) =>
                    {
                        builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options =>
                        {
                            options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                            options.MigrationsHistoryTable($"__{nameof(OrderStateDbContext)}");
                            options.EnableRetryOnFailure();
                        });
                    });
                    
                    r.UsePostgres();
                });

            cfg.UsingInMemory((context, config) =>
            {
                config.ConfigureEndpoints(context);
            });

            cfg.AddRider(rider =>
            {
                rider.AddSagaStateMachine<OrderStateMachine, OrderState>();

                rider.AddProducer<Payment.Submitted>(kafkaSettings.Endpoints.PaymentSubmitted);
                rider.AddProducer<Payment.DeadLetter>(kafkaSettings.Endpoints.PaymentDeadLetter);
                
                rider.AddProducer<Shipping.Submitted>(kafkaSettings.Endpoints.ShippingSubmitted);
                rider.AddProducer<Shipping.DeadLetter>(kafkaSettings.Endpoints.ShippingDeadLetter);
                
                rider.AddProducer<FinalEvent>(kafkaSettings.Endpoints.Final);

                rider.UsingKafka((context, k) =>
                {
                    k.Host($"{kafkaSettings.Host}:{kafkaSettings.Port}");

                    InitialAndFinalTopicEndpoint(context, k, kafkaSettings);
                    PaymentTopicEndpoint(context, k, kafkaSettings);
                    ShippingTopicEndpoint(context, k, kafkaSettings);
                });
            });
        });
            
        return services;
    }

    private static void InitialAndFinalTopicEndpoint(IRiderRegistrationContext context, IKafkaFactoryConfigurator k, BrokerSettings kafkaSettings)
    {
        k.TopicEndpoint<InitialEvent>(kafkaSettings.Endpoints.Initial, kafkaSettings.Endpoints.ConsumerGroup, e =>
        {
            e.ConfigureSaga<OrderState>(context);
        });

        // k.TopicEndpoint<FinalEvent>(kafkaSettings.Endpoints.Final, kafkaSettings.Endpoints.ConsumerGroup, e =>
        // {
        //     e.ConfigureSaga<OrderState>(context);
        // });
    }

    private static void PaymentTopicEndpoint(IRiderRegistrationContext context, IKafkaFactoryConfigurator k, BrokerSettings kafkaSettings)
    {
        // k.TopicEndpoint<Payment.Submitted>(kafkaSettings.Endpoints.PaymentSubmitted, kafkaSettings.Endpoints.ConsumerGroup, e =>
        // {
        //     e.ConfigureSaga<OrderState>(context);
        // });

        k.TopicEndpoint<Payment.Accepted>(kafkaSettings.Endpoints.PaymentAccepted, kafkaSettings.Endpoints.ConsumerGroup, e =>
        {
            e.ConfigureSaga<OrderState>(context);
        });

        k.TopicEndpoint<Payment.Cancelled>(kafkaSettings.Endpoints.PaymentCancelled, kafkaSettings.Endpoints.ConsumerGroup, e =>
        {
            e.ConfigureSaga<OrderState>(context);
        });

        // k.TopicEndpoint<Payment.DeadLetter>(kafkaSettings.Endpoints.PaymentDeadLetter, kafkaSettings.Endpoints.ConsumerGroup, e =>
        // {
        //     e.ConfigureSaga<OrderState>(context);
        // });
    }

    private static void ShippingTopicEndpoint(IRiderRegistrationContext context, IKafkaFactoryConfigurator k, BrokerSettings kafkaSettings)
    {
        // k.TopicEndpoint<Shipping.Submitted>(kafkaSettings.Endpoints.ShippingSubmitted, kafkaSettings.Endpoints.ConsumerGroup, e =>
        // {
        //     e.ConfigureSaga<OrderState>(context);
        // });

        k.TopicEndpoint<Shipping.Accepted>(kafkaSettings.Endpoints.ShippingAccepted, kafkaSettings.Endpoints.ConsumerGroup, e =>
        {
            e.ConfigureSaga<OrderState>(context);
        });

        k.TopicEndpoint<Shipping.Cancelled>(kafkaSettings.Endpoints.ShippingCancelled, kafkaSettings.Endpoints.ConsumerGroup, e =>
        {
            e.ConfigureSaga<OrderState>(context);
        });

        // k.TopicEndpoint<Shipping.DeadLetter>(kafkaSettings.Endpoints.ShippingDeadLetter, kafkaSettings.Endpoints.ConsumerGroup, e =>
        // {
        //     e.ConfigureSaga<OrderState>(context);
        // });
    }

    private static BrokerSettings BuildKafkaSettings(IConfiguration configuration)
    {
        var settings = configuration.GetSection("Broker").Get<BrokerSettings>();
        
        ArgumentNullException.ThrowIfNull(settings);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Host);
        ArgumentException.ThrowIfNullOrEmpty(settings.Port);
        ArgumentException.ThrowIfNullOrEmpty(settings.Username);
        ArgumentException.ThrowIfNullOrEmpty(settings.Password);
        
        ArgumentNullException.ThrowIfNull(settings.Endpoints);

        ArgumentNullException.ThrowIfNull(settings.Endpoints.ConsumerGroup);

        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.Initial);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentSubmitted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentAccepted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentCancelled);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.PaymentDeadLetter);
        
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingSubmitted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingAccepted);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingCancelled);
        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.ShippingDeadLetter);

        ArgumentException.ThrowIfNullOrEmpty(settings.Endpoints.Final);

        return settings;
    }
}
