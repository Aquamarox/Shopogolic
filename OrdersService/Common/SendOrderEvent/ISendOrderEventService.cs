using OrdersService.Models;

namespace OrdersService.Common.SendOrderEvent
{
    public interface ISendOrderEventService
    {
        Task SendOrderCreatedEventAsync(Order order, CancellationToken cancellationToken);
        Task SendOrderStatusUpdatedEventAsync(Order order, CancellationToken cancellationToken);
    }

}
