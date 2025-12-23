namespace PaymentsService.Models
{
    /// <summary>
    /// Финансовый счет пользователя. Содержит информацию о балансе и заблокированных средствах.
    /// </summary>
    public class Account
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public decimal HeldAmount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public int Version { get; set; }
    }

}
