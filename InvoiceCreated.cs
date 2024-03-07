using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MassTransitWithMediatR;

public record InvoiceCreated(Guid InvoiceId);

public class InvoiceCreatedConsumer : IConsumer<InvoiceCreated>
{
    private readonly ILogger<InvoiceCreatedConsumer> _logger;
    private readonly AppDbContext _appDbContext;

    public InvoiceCreatedConsumer(ILogger<InvoiceCreatedConsumer> logger, AppDbContext appDbContext)
    {
        _logger = logger;
        _appDbContext = appDbContext;
    }

    public async Task Consume(ConsumeContext<InvoiceCreated> context)
    {
        _logger.LogInformation("InvoiceCreatedConsumer");

        // this line fails because InvoiceCreated event is delivered before the invoice is saved to the database
        var invoice = await _appDbContext.Invoices.SingleAsync(x => x.Id == context.Message.InvoiceId);

        _logger.LogInformation("Invoice found!");
    }
}