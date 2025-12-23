namespace OrdersService.UseCases.CreateOrder
{
    public sealed record CreateOrderRequest(
    Guid UserId,
    List<OrderItemRequest> Items);
}
