using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;

namespace Samples.Orchestrator.Core.Infrastructure.Database;

public class OrderStateDbContext : SagaDbContext
{
    public OrderStateDbContext(DbContextOptions<OrderStateDbContext> options) : base(options)
    {
    }
    
    public DbSet<OrderState> OrderStates { get; set; }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get { yield return new OrderStateMap(); }
    }
}