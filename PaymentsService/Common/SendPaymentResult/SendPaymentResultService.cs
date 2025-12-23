using Contracts.DTOs;
using PaymentsService.Database;
using PaymentsService.Models;
using System.Text.Json;

namespace PaymentsService.Common.SendPaymentResult
{
    public class SendPaymentResultService(PaymentContext context) : ISendPaymentResultService
    {
        private readonly PaymentContext _context = context;

        public async Task SendPaymentResultAsync(
            Guid orderId,
            Guid userId,
            decimal amount,
            bool success,
            string? reason,
            CancellationToken cancellationToken)
        {
            object paymentEvent;
            string eventType;

            if (success)
            {
                paymentEvent = new PaymentProcessedEvent(orderId, userId, amount, DateTimeOffset.UtcNow);
                eventType = "PaymentProcessed";
            }
            else
            {
                paymentEvent = new PaymentFailedEvent(orderId, userId, amount, reason ?? "Unknown error", DateTimeOffset.UtcNow);
                eventType = "PaymentFailed";
            }

            string payload = JsonSerializer.Serialize(paymentEvent);

            OutboxMessage outboxMessage = new()
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                Payload = payload,
                IsSent = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _ = await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);
        }
    }

}
