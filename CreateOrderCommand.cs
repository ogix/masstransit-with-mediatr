using MassTransit;
using MediatR;

namespace MassTransitWithMediatR;

public class CreateOrderCommand : IRequest
{
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand>
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderCommandHandler(AppDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid()
        };

        _dbContext.Orders.Add(order);

        await _publishEndpoint.Publish(new OrderCreated(order.Id), cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}