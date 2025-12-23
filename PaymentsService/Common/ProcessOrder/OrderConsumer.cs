using Confluent.Kafka;
using Contracts.DTOs;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using PaymentsService.Models;
using System.Text.Json;

namespace PaymentsService.Common.ProcessOrder
{
    /// <summary>
    /// Фоновый сервис для прослушивания топика заказов в Kafka.
    /// Реализует паттерн Transactional Inbox, сохраняя входящие сообщения в базу для обеспечения идемпотентности.
    /// </summary>
    public class OrderConsumer(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderConsumer> logger,
        IConsumer<string, string> consumer,
        string topic) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<OrderConsumer> _logger = logger;
        private readonly IConsumer<string, string> _consumer = consumer;
        private readonly string _topic = topic;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderConsumer starting, waiting for migrations...");

            // Ждем миграции (максимум 120 секунд)
            if (!MigrationRunner.WaitForMigrations(TimeSpan.FromSeconds(120), _logger))
            {
                _logger.LogError("Migrations did not complete in time, OrderConsumer will not start");
                return;
            }

            _consumer.Subscribe(_topic);
            _logger.LogInformation("OrderConsumer started listening to {Topic} (migrations ready)", _topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<string, string> consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult.Message?.Value == null)
                    {
                        continue;
                    }

                    await ProcessMessageAsync(consumeResult.Message, stoppingToken);
                    _consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task ProcessMessageAsync(Message<string, string> message, CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            PaymentContext context = scope.ServiceProvider.GetRequiredService<PaymentContext>();

            try
            {
                OrderCreatedEvent orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message.Value);
                if (orderCreatedEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize OrderCreatedEvent");
                    return;
                }

                // Создаем MessageId для идемпотентности (OrderId + EventType)
                string messageId = $"{orderCreatedEvent.OrderId}-OrderCreated";

                // Проверяем, не обрабатывали ли мы это сообщение уже
                InboxMessage? existingMessage = await context.InboxMessages
                    .FirstOrDefaultAsync(m => m.MessageId == messageId, cancellationToken);

                if (existingMessage != null)
                {
                    _logger.LogInformation("Message {MessageId} already processed, skipping", messageId);
                    return;
                }

                // Сохраняем в Inbox
                InboxMessage inboxMessage = new()
                {
                    Id = Guid.NewGuid(),
                    MessageId = messageId,
                    EventType = "OrderCreated",
                    Payload = message.Value,
                    IsProcessed = false,
                    ReceivedAt = DateTimeOffset.UtcNow
                };

                _ = await context.InboxMessages.AddAsync(inboxMessage, cancellationToken);
                _ = await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("OrderCreatedEvent for order {OrderId} saved to inbox", orderCreatedEvent.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message");
                throw;
            }
        }
    }
}