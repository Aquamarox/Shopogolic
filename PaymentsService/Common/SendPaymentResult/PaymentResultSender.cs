using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using PaymentsService.Models;

namespace PaymentsService.Common.SendPaymentResult
{
    /// <summary>
    /// Фоновый сервис (воркер), который отправляет накопленные сообщения о результатах платежей из Outbox в Kafka.
    /// Гарантирует доставку статуса оплаты обратно в сервис заказов.
    /// </summary>
    public class PaymentResultSender(
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentResultSender> logger,
        IProducer<string, string> producer,
        Dictionary<string, string> topics) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<PaymentResultSender> _logger = logger;
        private readonly IProducer<string, string> _producer = producer;
        private readonly Dictionary<string, string> _topics = topics;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PaymentResultSender starting, waiting for migrations...");

            // Ждем миграции (максимум 120 секунд)
            if (!MigrationRunner.WaitForMigrations(TimeSpan.FromSeconds(120), _logger))
            {
                _logger.LogError("Migrations did not complete in time, PaymentResultSender will not start");
                return;
            }

            _logger.LogInformation("PaymentResultSender started (migrations ready)");

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
            PaymentContext context = scope.ServiceProvider.GetRequiredService<PaymentContext>();

            List<Models.OutboxMessage> messages = await context.OutboxMessages
                .Where(m => !m.IsSent)
                .OrderBy(m => m.CreatedAt)
                .Take(10)
                .ToListAsync(cancellationToken);

            foreach (OutboxMessage message in messages)
            {
                try
                {
                    string topic;
                    if (message.EventType == "PaymentProcessed")
                    {
                        topic = _topics.GetValueOrDefault("PaymentProcessed", "payment-processed");
                    }
                    else if (message.EventType == "PaymentFailed")
                    {
                        topic = _topics.GetValueOrDefault("PaymentFailed", "payment-failed");
                    }
                    else
                    {
                        _logger.LogWarning("Unknown event type: {EventType}", message.EventType);
                        continue;
                    }

                    DeliveryResult<string, string> deliveryResult = await _producer.ProduceAsync(
                        topic,
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
                        _logger.LogInformation("Payment result sent to {Topic}", topic);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send message {MessageId}", message.Id);
                }
            }

            if (messages.Count != 0)
            {
                _ = await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}