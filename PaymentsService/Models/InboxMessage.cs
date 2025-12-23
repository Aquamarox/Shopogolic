namespace PaymentsService.Models
{
    /// <summary>
    /// Сообщение, полученное извне и сохраненное для гарантированной обработки.
    /// Используется для реализации паттерна Transactional Inbox и обеспечения идемпотентности.
    /// </summary>
    public class InboxMessage
    {
        public Guid Id { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public bool IsProcessed { get; set; }
        public DateTimeOffset ReceivedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
    }

}
