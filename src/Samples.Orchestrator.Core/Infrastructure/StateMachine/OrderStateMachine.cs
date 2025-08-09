using System.Text.Json;
using MassTransit;
using Samples.Orchestrator.Core.Domain.Events.Final;
using Samples.Orchestrator.Core.Domain.Events.Start;
using Samples.Orchestrator.Core.Infrastructure.Factories;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Core.Infrastructure.StateMachine;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    #region States
    public State InitialState { get; private set; }

    public State PaymentSubmittedState { get; private set; }
    public State PaymentAcceptedState { get; private set; }
    public State PaymentCancelledState { get; private set; }
    public State PaymentDeadLetterState { get; private set; }

    public State ShippingSubmittedState { get; private set; }
    public State ShippingCancelledState { get; private set; }
    public State ShippingDeadLetterState { get; private set; }

    public State FinalState { get; private set; }
    #endregion

    #region Events
    public Event<InitialEvent> InitialEvent { get; private set; }

    public Event<Payment.Submitted> PaymentSubmittedEvent { get; private set; }
    public Event<Payment.Accepted> PaymentAcceptedEvent { get; private set; }
    public Event<Payment.Cancelled> PaymentCancelledEvent { get; private set; }
    public Event<Payment.DeadLetter> PaymentDeadLetterEvent { get; private set; }

    public Event<Shipping.Submitted> ShippingSubmittedEvent { get; private set; }
    public Event<Shipping.Accepted> ShippingAcceptedEvent { get; private set; }
    public Event<Shipping.Cancelled> ShippingCancelledEvent { get; private set; }
    public Event<Shipping.DeadLetter> ShippingDeadLetterEvent { get; private set; }
    #endregion

    public OrderStateMachine(ILogger<OrderStateMachine> logger, IConfiguration configuration, IBrokerSettingsFactory brokerSettingsFactory)
    {
        var settings = brokerSettingsFactory.Create(configuration);
        
        InstanceState(x => x.CurrentState);

        #region Configure Events

        Event(() => InitialEvent);

        Event(() => PaymentSubmittedEvent);
        Event(() => PaymentAcceptedEvent);
        Event(() => PaymentCancelledEvent);
        Event(() => PaymentDeadLetterEvent);
        
        Event(() => ShippingSubmittedEvent);
        Event(() => ShippingAcceptedEvent);
        Event(() => ShippingCancelledEvent);
        Event(() => ShippingDeadLetterEvent);

        #endregion

        #region Configure States

        State(() => InitialState);

        State(() => PaymentSubmittedState);
        State(() => PaymentAcceptedState);
        State(() => PaymentCancelledState);
        State(() => PaymentDeadLetterState);

        State(() => ShippingSubmittedState);
        State(() => ShippingCancelledState);
        State(() => ShippingDeadLetterState);

        State(() => FinalState);

        #endregion

        #region State Machine

        Initially(
            When(InitialEvent)
                .Then(context =>
                {
                    context.Saga.Initialize(context.Message);
                    LogMessage(logger, context.Message);
                })
                .TransitionTo(InitialState)
        );

        During(Initial,
            When(PaymentSubmittedEvent)
                .IfElse(ctx => ctx.Message.RetryCount > 3,
                    thenBinder => thenBinder
                        .PublishAsync(ctx => ctx.Init<Payment.DeadLetter>(new
                        {
                            ctx.Saga.CorrelationId,
                            ctx.Saga.CurrentState,
                            ctx.Message.RetryCount,
                            ctx.Message.Payload,
                            ctx.Message.CreatedAt
                        }))
                        .Then(ctx => LogMessage(logger, ctx.Message))
                        .TransitionTo(PaymentDeadLetterState),

                    elseBinder => elseBinder
                        .PublishAsync(ctx => ctx.Init<Payment.Submitted>(new
                        {
                            ctx.Saga.CorrelationId,
                            ctx.Saga.CurrentState,
                            ctx.Message.Payload,
                            ctx.Message.CreatedAt
                        }))
                        .Then(ctx => LogMessage(logger, ctx.Message))
                        .TransitionTo(PaymentSubmittedState)
                )
        );

        During(PaymentSubmittedState,
            When(PaymentAcceptedEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(PaymentAcceptedState),

            When(PaymentCancelledEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(PaymentCancelledState)
        );

        During(PaymentAcceptedState,
            When(ShippingSubmittedEvent)
                .IfElse(ctx => ctx.Message.RetryCount > 3,
                    thenBinder => thenBinder
                        .PublishAsync(ctx => ctx.Init<Shipping.DeadLetter>(new
                        {
                            ctx.Saga.CorrelationId,
                            ctx.Message.CurrentState,
                            ctx.Message.RetryCount,
                            ctx.Message.Payload,
                            ctx.Message.CreatedAt
                        }))
                        .Then(ctx => LogMessage(logger, ctx.Message))
                        .TransitionTo(ShippingDeadLetterState),

                    elseBinder => elseBinder
                        .PublishAsync(ctx => ctx.Init<Shipping.Submitted>(new
                        {
                            ctx.Saga.CorrelationId,
                            ctx.Message.CurrentState,
                            ctx.Message.Payload,
                            ctx.Message.CreatedAt
                        }))
                        .Then(ctx => LogMessage(logger, ctx.Message))
                        .TransitionTo(ShippingSubmittedState)
                )
        );

        During(ShippingSubmittedState,
            When(ShippingAcceptedEvent)
                .Then(context => LogMessage(logger, context.Message))
                .PublishAsync(ctx => ctx.Init<FinalEvent>(new
                {
                    ctx.CorrelationId,
                    ctx.Message.CurrentState,
                    ctx.Message.Payload,
                    ctx.Message.CreatedAt
                }))
                .TransitionTo(FinalState),

            When(ShippingCancelledEvent)
                .Then(context => LogMessage(logger, context.Message))
                .TransitionTo(ShippingCancelledState)
        );
        
        #endregion
    }
    
    private static void LogMessage<T>(ILogger logger, T message)
    {
        logger.LogInformation("Message: {Message} processed", JsonSerializer.Serialize(message));
    }
}