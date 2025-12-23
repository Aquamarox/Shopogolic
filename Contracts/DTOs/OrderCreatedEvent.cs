namespace Contracts.DTOs
{
    public sealed record OrderCreatedEvent(
        Guid OrderId,
        Guid UserId,
        decimal TotalAmount,
        List<OrderItemDto> Items,
        DateTimeOffset CreatedAt);
}
