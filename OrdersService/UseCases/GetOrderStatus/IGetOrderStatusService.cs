using OrdersService.Models;

namespace OrdersService.UseCases.GetOrderStatus
{
    public interface IGetOrderStatusService
    {
        Task<Order?> GetOrderStatusAsync(Guid orderId, Guid userId, CancellationToken cancellationToken);
    }
}
