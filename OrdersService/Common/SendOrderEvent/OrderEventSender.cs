using OrdersService.Database;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using OrdersService.Models;

namespace OrdersService.Common.SendOrderEvent
{
    /// <summary>
    /// Реализация паттерна Transactional Outbox.
    /// Фоновый воркер, который сканирует таблицу OutboxMessages и отправляет события в Kafka.
    /// </summary>
    public class OrderEventSender(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderEventSender> logger,
        IProducer<string, string> producer,
        string topic) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<OrderEventSender> _logger = logger;
        private readonly IProducer<string, string> _producer = producer;
        private readonly string _topic = topic;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderEventSender started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            OrderContext context = scope.ServiceProvider.GetRequiredService<OrderContext>();

            List<OutboxMessage> messages = await context.OutboxMessages
                .Where(m => !m.IsSent)
                .OrderBy(m => m.CreatedAt)
                .Take(10)
                .ToListAsync(cancellationToken);

            foreach (OutboxMessage message in messages)
            {
                try
                {
                    DeliveryResult<string, string> deliveryResult = await _producer.ProduceAsync(
                        _topic,
                        new Message<string, string>
                        {
                            Key = message.Id.ToString(),
                            Value = message.Payload
                        },
                        cancellationToken);

                    if (deliveryResult.Status == PersistenceStatus.Persisted)
                    {
                        message.IsSent = true;
                        message.SentAt = DateTimeOffset.UtcNow;
                        _logger.LogInformation("Message {MessageId} sent to Kafka", message.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send message {MessageId} to Kafka", message.Id);
                }
            }

            if (messages.Count != 0)
            {
                _ = await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

}
