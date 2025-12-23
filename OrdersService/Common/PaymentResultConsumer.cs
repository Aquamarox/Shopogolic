using Confluent.Kafka;
using Contracts.DTOs;
using Microsoft.EntityFrameworkCore;
using OrdersService.Database;
using OrdersService.Models;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using OrdersService.Common;

namespace OrdersService.Common
{
    /// <summary>
    /// Фоновый сервис, который прослушивает Kafka на предмет результатов оплаты (успех/ошибка).
    /// Обновляет статус заказа в базе данных и уведомляет пользователя через SignalR.
    /// </summary>
    public class PaymentResultConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentResultConsumer> logger,
        IConsumer<string, string> consumer,
        IConfiguration configuration,
        IHubContext<OrderHub> hubContext) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<PaymentResultConsumer> _logger = logger;
        private readonly IConsumer<string, string> _consumer = consumer;
        private readonly IHubContext<OrderHub> _hubContext = hubContext;
        private readonly string _paymentProcessedTopic = configuration["Kafka:Topic:PaymentProcessed"] ?? "payment-processed";
        private readonly string _paymentFailedTopic = configuration["Kafka:Topic:PaymentFailed"] ?? "payment-failed";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe([_paymentProcessedTopic, _paymentFailedTopic]);
            _logger.LogInformation("PaymentResultConsumer started listening");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<string, string> consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult.Message?.Value == null)
                    {
                        continue;
                    }

                    await ProcessMessageAsync(consumeResult.Topic, consumeResult.Message, stoppingToken);
                    _consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in PaymentResultConsumer");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task ProcessMessageAsync(string topic, Message<string, string> message, CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            OrderContext context = scope.ServiceProvider.GetRequiredService<OrderContext>();

            try
            {
                if (topic == _paymentProcessedTopic)
                {
                    PaymentProcessedEvent? paymentEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(message.Value);
                    if (paymentEvent != null)
                    {
                        await UpdateOrderStatusAsync(context, paymentEvent.OrderId, OrderStatus.PaymentCompleted, cancellationToken);
                    }
                }
                else if (topic == _paymentFailedTopic)
                {
                    PaymentFailedEvent? paymentEvent = JsonSerializer.Deserialize<PaymentFailedEvent>(message.Value);
                    if (paymentEvent != null)
                    {
                        await UpdateOrderStatusAsync(context, paymentEvent.OrderId, OrderStatus.PaymentFailed, cancellationToken);
                    }
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Error processing message"); }
        }

        /// <summary>
        /// Обновляет статус заказа в базе данных и отправляет Push-уведомление через WebSocket.
        /// </summary>
        private async Task UpdateOrderStatusAsync(OrderContext context, Guid orderId, OrderStatus status, CancellationToken cancellationToken)
        {
            Order? order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTimeOffset.UtcNow;
                _ = await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);

                await _hubContext.Clients.Group(order.UserId.ToString()).SendAsync("ReceiveOrderStatusUpdate", new
                {
                    OrderId = order.Id,
                    Status = status.ToString(),
                    Message = status == OrderStatus.PaymentCompleted ? "Оплата успешно получена!" : "Ошибка при оплате заказа."
                }, cancellationToken);
            }
        }

        public override void Dispose()
        {
            _consumer?.Close();
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}