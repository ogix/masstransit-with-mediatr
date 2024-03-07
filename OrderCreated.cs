using MassTransit;
using MediatR;

namespace MassTransitWithMediatR;

public record OrderCreated(Guid OrderId);

public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    private readonly IMediator _mediator;


    public OrderCreatedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        await _mediator.Send(new CreateInvoiceCommand(context.Message.OrderId));
    }
}