using OrdersService.Models;

namespace OrdersService.UseCases.CreateOrder
{
    public sealed record CreateOrderResponse(
    Guid OrderId,
    OrderStatus Status,
    decimal TotalAmount);
}
