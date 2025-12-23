using OrdersService.Database;
using System.Text.Json;
using Contracts.DTOs;
using OrdersService.Models;

namespace OrdersService.Common.SendOrderEvent
{
    public class SendOrderEventService(OrderContext context) : ISendOrderEventService
    {
        private readonly OrderContext _context = context;

        public async Task SendOrderCreatedEventAsync(Order order, CancellationToken cancellationToken)
        {
            OrderCreatedEvent orderCreatedEvent = new(
                order.Id,
                order.UserId,
                order.TotalAmount,
                order.Items.Select(i => new OrderItemDto(
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.Price)).ToList(),
                order.CreatedAt
            );

            string payload = JsonSerializer.Serialize(orderCreatedEvent);

            OutboxMessage outboxMessage = new()
            {
                Id = Guid.NewGuid(),
                EventType = "OrderCreated",
                Payload = payload,
                IsSent = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _ = await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SendOrderStatusUpdatedEventAsync(Order order, CancellationToken cancellationToken)
        {
            // Реализация для обновления статуса заказа (опционально)
            await Task.CompletedTask;
        }
    }
}

