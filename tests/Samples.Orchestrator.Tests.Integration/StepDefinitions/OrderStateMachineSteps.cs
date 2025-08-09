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

namespace Samples.Orchestrator.Tests.Integration.StepDefinitions;

[Binding]
public class OrderStateMachineSteps
{
    private ServiceProvider _provider = default!;
    private ITestHarness _harness = default!;
    private ISagaStateMachineTestHarness<OrderStateMachine, OrderState> _sagaHarness = default!;
    private Guid _sagaId;
    private JsonObject _payload;

    [BeforeScenario]
    public async Task Setup()
    {
        var configuration = new ConfigurationBuilder().Build();

        var brokerFactoryMock = new Mock<IBrokerSettingsFactory>();
        
        _provider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<IBrokerSettingsFactory>(brokerFactoryMock.Object)
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderState>()
                   .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        _sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderState>();
        _payload = new Faker<JsonObject>()
       .CustomInstantiator(f => new JsonObject
       {
           ["OrderId"] = f.Random.Int(1, 1000),
           ["Amount"] = f.Finance.Amount(10, 5000),
           ["Currency"] = f.Finance.Currency().Code,
           ["PaymentMethod"] = f.PickRandom(new[] { "CreditCard", "PayPal", "BankTransfer" })
       })
       .Generate();
        await _harness.Start();
    }

    [AfterScenario]
    public async Task Cleanup()
    {
        if (_provider != null)
            await _provider.DisposeAsync();
    }

    [Given("que inicio uma nova saga de pedido")]
    public void GivenQueInicioUmaNovaSagaDePedido()
    {
        _sagaId = NewId.NextGuid();
    }

    [When("envio um Payment.Submitted válido")]
    public async Task WhenEnvioUmPaymentSubmittedValido()
    {
        await _harness.Bus.Publish(new Payment.Submitted
        {
            CorrelationId = _sagaId,
            CurrentState = "PaymentSubmitted",
            Payload = _payload
        });
    }

    [When("envio um Payment.Accepted")]
    public async Task WhenEnvioUmPaymentAccepted()
    {
        await _harness.Bus.Publish(new Payment.Accepted
        {
            CorrelationId = _sagaId,
            CurrentState = "PaymentAccepted",
            Payload = _payload
        });
    }

    [When("envio um Shipping.Submitted válido")]
    public async Task WhenEnvioUmShippingSubmittedValido()
    {
        await _harness.Bus.Publish(new Shipping.Submitted
        {
            CorrelationId = _sagaId,
            CurrentState = "ShippingSubmitted",
            Payload = _payload
        });
    }

    [When("envio um Shipping.Accepted")]
    public async Task WhenEnvioUmShippingAccepted()
    {
        await _harness.Bus.Publish(new Shipping.Accepted
        {
            CorrelationId = _sagaId,
            CurrentState = "ShippingAccepted",
            Payload = _payload
        });
    }

    [Then("o estado final deve ser FinalState")]
    public async Task ThenOEstadoFinalDeveSerFinalState()
    {
        var instance = await _sagaHarness.Exists(_sagaId, x => x.FinalState);
        instance.Should().NotBeNull();
    }

    [Then("um evento FinalEvent deve ser publicado")]
    public void ThenUmEventoFinalEventDeveSerPublicado()
    {
        _harness.Published.Select<FinalEvent>().Any().Should().BeTrue();
    }
}