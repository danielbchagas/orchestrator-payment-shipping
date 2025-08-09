using Bogus;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Samples.Orchestrator.Core.Domain.Events.Final;
using Samples.Orchestrator.Core.Infrastructure.Factories;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;
using System.Text.Json.Nodes;
using Payment = Samples.Orchestrator.Core.Domain.Events.Payment;
using Shipping = Samples.Orchestrator.Core.Domain.Events.Shipping;

namespace Samples.Orchestrator.Tests.StateMachine;

public class OrderStateMachineTests
{
    private IConfiguration Configuration;
    private Mock<IBrokerSettingsFactory> BrokerSettingsFactory;

    private const string InitialEvent = "Initial";

    private const string PaymentSubmitted = "PaymentSubmitted";
    private const string PaymentAccepted = "PaymentAccepted";
    private const string PaymentCancelled = "PaymentCancelled";
    private const string PaymentDeadLetter = "PaymentDeadLetter";

    private const string ShippingSubmitted = "ShippingSubmitted";
    private const string ShippingAccepted = "ShippingAccepted";
    private const string ShippingCancelled = "ShippingCancelled";
    private const string ShippingDeadLetter = "ShippingDeadLetter";

    private JsonObject Payload { get; }

    public OrderStateMachineTests()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        BrokerSettingsFactory = new Mock<IBrokerSettingsFactory>();

        Payload = new Faker<JsonObject>()
       .CustomInstantiator(f => new JsonObject
       {
           ["OrderId"] = f.Random.Int(1, 1000),
           ["Amount"] = f.Finance.Amount(10, 5000),
           ["Currency"] = f.Finance.Currency().Code,
           ["PaymentMethod"] = f.PickRandom(new[] { "CreditCard", "PayPal", "BankTransfer" })
       })
       .Generate();
    }

    [Fact]
    public async Task Should_TransitionTo_PaymentAccepted_When_PaymentAcceptedEventReceived()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddSingleton(BrokerSettingsFactory.Object)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState = PaymentAccepted, Payload = Payload });

        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentAcceptedState);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeFalse();

        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_PaymentCancelled_When_PaymentCancelledEventReceived()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddSingleton(BrokerSettingsFactory.Object)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Cancelled { CorrelationId = sagaId, CurrentState = PaymentCancelled, Payload = Payload });

        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentCancelledState);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Cancelled>().Any().Should().BeTrue();

        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_Final_When_ShippingAcceptedEventReceived()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddSingleton(BrokerSettingsFactory.Object)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState = PaymentAccepted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, CurrentState = ShippingSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Accepted { CorrelationId = sagaId, CurrentState = ShippingAccepted, Payload = Payload });

        var instance = await sagaHarness.Exists(sagaId, x => x.FinalState);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Accepted>().Any().Should().BeTrue();

        _harness.Published.Select<FinalEvent>().Any().Should().BeTrue();

        await _harness.Stop();
    }


    [Fact]
    public async Task Should_TransitionTo_ShippingCancelled_When_ShippingCancelled()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddSingleton(BrokerSettingsFactory.Object)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState = PaymentAccepted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, CurrentState = ShippingSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Cancelled { CorrelationId = sagaId, CurrentState = ShippingCancelled, Payload = Payload });

        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingCancelledState);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Cancelled>().Any().Should().BeTrue();

        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_PaymentDeadLetter_When_PaymentSubmittedRetryExceeded()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddSingleton(BrokerSettingsFactory.Object)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted
        {
            CorrelationId = sagaId,
            CurrentState = PaymentSubmitted,
            Payload = Payload,
            RetryCount = 4
        });

        var instance = await sagaHarness.Exists(sagaId, x => x.PaymentDeadLetterState);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        _harness.Published.Select<Payment.DeadLetter>().Any().Should().BeTrue();

        await _harness.Stop();
    }

    [Fact]
    public async Task Should_TransitionTo_ShippingDeadLetter_When_ShippingSubmittedRetryExceeded()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddSingleton(Configuration)
            .AddSingleton(BrokerSettingsFactory.Object)
            .AddMassTransitTestHarness(config =>
            {
                config.AddSagaStateMachine<OrderStateMachine, OrderState>();
            })
            .BuildServiceProvider(true);

        var _harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        await _harness.Start();
        var sagaId = NewId.NextGuid();

        // Act
        await _harness.Bus.Publish(new Payment.Submitted { CorrelationId = sagaId, CurrentState = PaymentSubmitted, Payload = Payload });
        await _harness.Bus.Publish(new Payment.Accepted { CorrelationId = sagaId, CurrentState = PaymentAccepted, Payload = Payload });
        await _harness.Bus.Publish(new Shipping.Submitted { CorrelationId = sagaId, CurrentState = ShippingSubmitted, Payload = Payload, RetryCount = 4 });

        var instance = await sagaHarness.Exists(sagaId, x => x.ShippingDeadLetterState);

        // Assert
        instance.Should().NotBeNull();
        sagaHarness.Consumed.Select<Payment.Submitted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Payment.Accepted>().Any().Should().BeTrue();
        sagaHarness.Consumed.Select<Shipping.Submitted>().Any().Should().BeTrue();
        _harness.Published.Select<Shipping.DeadLetter>().Any().Should().BeTrue();

        await _harness.Stop();
    }
}
