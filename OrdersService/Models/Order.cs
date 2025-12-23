namespace OrdersService.Models
{
    /// <summary>
    /// Сущность заказа, содержащая информацию о пользователе, сумме и текущем статусе.
    /// </summary>
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public List<OrderItem> Items { get; set; } = [];
    }
}
