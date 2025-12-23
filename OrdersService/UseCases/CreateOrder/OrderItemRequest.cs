namespace OrdersService.UseCases.CreateOrder
{
    public sealed record OrderItemRequest(
        Guid ProductId,
        string ProductName,
        int Quantity,
        decimal Price);
}
