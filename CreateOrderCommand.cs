using MassTransit;
using MediatR;

namespace MassTransitWithMediatR;

public class CreateOrderCommand : IRequest
{
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand>
{
    private readonly AppDbContext _appDbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderCommandHandler(AppDbContext appDbContext, IPublishEndpoint publishEndpoint)
    {
        _appDbContext = appDbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // this simulates transaction middleware
        await using var transaction = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        var order = new Order
        {
            Id = Guid.NewGuid()
        };

        _appDbContext.Orders.Add(order);

        // this also simulates publishing integration events from domain event handlers and then saving the changes to the database
        await _publishEndpoint.Publish(new OrderCreated(order.Id), cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}