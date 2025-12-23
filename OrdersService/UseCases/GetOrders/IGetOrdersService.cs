using OrdersService.Models;

namespace OrdersService.UseCases.GetOrders
{
    public interface IGetOrdersService
    {
        Task<List<Order>> GetOrdersAsync(Guid userId, CancellationToken cancellationToken);
    }
}
