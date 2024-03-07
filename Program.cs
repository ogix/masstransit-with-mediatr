using System.Reflection;
using MassTransit;
using MassTransitWithMediatR;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(x =>
{
    x.UseSqlite("Data Source=Database.db");
});

builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });

    x.AddConsumers(Assembly.GetExecutingAssembly());

    x.AddEntityFrameworkOutbox<AppDbContext>(o =>
    {
        // configure which database lock provider to use (Postgres, SqlServer, or MySql)
        o.UseSqlite();

        // enable the bus outbox
        o.UseBusOutbox();
    });

    x.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        // this line enables transactional outbox for consumers but does not work with explicit database transactions
        // cfg.UseEntityFrameworkOutbox<AppDbContext>(context);
    });
});

var host = builder.Build();

using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();

    var mediator = serviceScope.ServiceProvider.GetRequiredService<IMediator>();
    await mediator.Send(new CreateOrderCommand());
}

host.Run();

