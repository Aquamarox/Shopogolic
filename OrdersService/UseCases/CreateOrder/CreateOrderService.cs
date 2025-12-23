using OrdersService.Common.SendOrderEvent;
using OrdersService.Database;
using OrdersService.Models;

namespace OrdersService.UseCases.CreateOrder
{
    public class CreateOrderService(
        OrderContext context,
        ISendOrderEventService sendOrderEventService) : ICreateOrderService
    {
        private readonly OrderContext _context = context;
        private readonly ISendOrderEventService _sendOrderEventService = sendOrderEventService;

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Создаем заказ
                Order order = new()
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    TotalAmount = request.Items.Sum(i => i.Price * i.Quantity),
                    Status = OrderStatus.PaymentPending,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Items = request.Items.Select(i => new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                };

                _ = await _context.Orders.AddAsync(order, cancellationToken);
                _ = await _context.SaveChangesAsync(cancellationToken);

                // Добавляем событие в Outbox
                await _sendOrderEventService.SendOrderCreatedEventAsync(order, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return new CreateOrderResponse(order.Id, order.Status, order.TotalAmount);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

}
