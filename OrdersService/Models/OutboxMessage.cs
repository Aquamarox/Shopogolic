namespace OrdersService.Models
{
    /// <summary>
    /// Модель сообщения для паттерна Transactional Outbox. 
    /// Хранит события, которые должны быть гарантированно отправлены в брокер сообщений.
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
