namespace PaymentsService.Models
{
    /// <summary>
    /// Сообщение, подготовленное для отправки в брокер (Kafka).
    /// Часть паттерна Transactional Outbox для обеспечения гарантии доставки At-Least-Once.
    /// </summary>
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public bool IsSent { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? SentAt { get; set; }
    }
}
