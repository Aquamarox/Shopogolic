using OrdersService.Models;

namespace OrdersService.Common.SendOrderEvent
{
    /// <summary>
    /// Интерфейс сервиса для подготовки и сохранения событий заказов в таблицу Outbox.
    /// </summary>
    public interface ISendOrderEventService
    {
        /// <summary>
        /// Формирует событие OrderCreated и сохраняет его в таблицу Outbox для последующей отправки.
        /// </summary>
        Task SendOrderCreatedEventAsync(Order order, CancellationToken cancellationToken);
        Task SendOrderStatusUpdatedEventAsync(Order order, CancellationToken cancellationToken);
    }

}
