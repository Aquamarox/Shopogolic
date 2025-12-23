namespace PaymentsService.Models
{
    public class InboxMessage
    {
        public Guid Id { get; set; }
        public string MessageId { get; set; } = string.Empty; // Идентификатор сообщения для идемпотентности
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public bool IsProcessed { get; set; }
        public DateTimeOffset ReceivedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
    }

}
