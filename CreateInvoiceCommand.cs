using MassTransit;
using MediatR;

namespace MassTransitWithMediatR;

public record CreateInvoiceCommand(Guid OrderId) : IRequest;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand>
{
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateInvoiceCommandHandler(
        ILogger<CreateInvoiceCommandHandler> logger,
        AppDbContext appDbContext,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _appDbContext = appDbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // this simulates transaction middleware
        await using var transaction = await _appDbContext.Database.BeginTransactionAsync(cancellationToken);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId
        };

        _appDbContext.Invoices.Add(invoice);

        // this also simulates publishing integration events from domain event handlers and then saving the changes to the database
        await _publishEndpoint.Publish(new InvoiceCreated(invoice.Id), cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        // add some delay to reproduce the issue everytime
        await Task.Delay(1000, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}