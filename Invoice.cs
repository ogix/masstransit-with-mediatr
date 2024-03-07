namespace MassTransitWithMediatR;

public class Invoice
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
}