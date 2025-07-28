using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Samples.Orchestrator.Core.Infrastructure.StateMachine;

namespace Samples.Orchestrator.Core.Infrastructure.Database;

public class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.HasKey(x => x.CorrelationId);
        entity.Property(x => x.CurrentState).HasColumnType("VARCHAR").HasMaxLength(64);
        entity.Property(x => x.Payload).HasColumnType("VARCHAR").HasMaxLength(1024).HasJsonConversion();
        entity.Property(x => x.CreatedAt).HasColumnType("DATE");
        entity.Property(x => x.RetryCount).HasColumnType("INT");
    }
}
