using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samples.Orchestrator.Core.Infrastructure.Database;
using Samples.Orchestrator.Core.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMasstransit(builder.Configuration);
builder.Services.AddEntityFramework(builder.Configuration);
builder.Services.AddDependencyInjection(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/order-states/{page}/{pageSize}", async ([FromServices] OrderStateDbContext context, [FromQuery] int page, int pageSize) =>
    {
        var result = await context.OrderStates.AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return result switch
        {
            null => Results.NotFound(),
            _ => Results.Ok(result)
        };
    })
    .WithName("Get Orders")
    .WithOpenApi();

app.Run();