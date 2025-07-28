using Microsoft.EntityFrameworkCore;
using Samples.Orchestrator.Core.Infrastructure.Database;

namespace Samples.Orchestrator.Core.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DbContext, OrderStateDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), options =>
            {
                options.EnableRetryOnFailure();
            });
        });

        return services;
    }
}
