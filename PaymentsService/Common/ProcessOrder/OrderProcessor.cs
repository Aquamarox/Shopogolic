using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using PaymentsService.Models;
using PaymentsService.UseCases.ProcessPayment;
using System.Text.Json;

namespace PaymentsService.Common.ProcessOrder
{
    /// <summary>
    /// Фоновый сервис, который обрабатывает сообщения из таблицы Inbox.
    /// Инициирует процесс списания средств и передает результат в сервис отправки уведомлений.
    /// </summary>
    public class OrderProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OrderProcessor> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<OrderProcessor> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderProcessor starting, waiting for migrations...");

            // Ждем миграции (максимум 120 секунд)
            if (!MigrationRunner.WaitForMigrations(TimeSpan.FromSeconds(120), _logger))
            {
                _logger.LogError("Migrations did not complete in time, OrderProcessor will not start");
                return;
            }

            _logger.LogInformation("OrderProcessor started (migrations ready)");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessInboxMessagesAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing inbox messages");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task ProcessInboxMessagesAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            PaymentContext context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
            IProcessPaymentService processPaymentService = scope.ServiceProvider.GetRequiredService<IProcessPaymentService>();
            SendPaymentResult.ISendPaymentResultService sendPaymentResultService = scope.ServiceProvider.GetRequiredService<SendPaymentResult.ISendPaymentResultService>();

            List<InboxMessage> messages = await context.InboxMessages
                .Where(m => !m.IsProcessed)
                .OrderBy(m => m.ReceivedAt)
                .Take(10)
                .ToListAsync(cancellationToken);

            foreach (InboxMessage message in messages)
            {
                try
                {
                    _logger.LogInformation("Processing message {MessageId}", message.MessageId);

                    Contracts.DTOs.OrderCreatedEvent? orderCreatedEvent = JsonSerializer.Deserialize<Contracts.DTOs.OrderCreatedEvent>(message.Payload);
                    if (orderCreatedEvent == null)
                    {
                        _logger.LogWarning("Failed to deserialize message {MessageId}", message.MessageId);
                        message.IsProcessed = true;
                        message.ProcessedAt = DateTimeOffset.UtcNow;
                        continue;
                    }

                    ProcessPaymentResult result = await processPaymentService.ProcessPaymentAsync(
                        orderCreatedEvent.OrderId,
                        orderCreatedEvent.UserId,
                        orderCreatedEvent.TotalAmount,
                        cancellationToken);

                    await sendPaymentResultService.SendPaymentResultAsync(
                        orderCreatedEvent.OrderId,
                        orderCreatedEvent.UserId,
                        orderCreatedEvent.TotalAmount,
                        result.Success,
                        result.Reason,
                        cancellationToken);

                    message.IsProcessed = true;
                    message.ProcessedAt = DateTimeOffset.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
                }
            }

            if (messages.Count != 0)
            {
                _ = await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}