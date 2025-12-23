namespace OrdersService.Models
{
    /// <summary>
    /// Перечисление возможных состояний заказа в процессе его жизненного цикла.
    /// </summary>
    public enum OrderStatus
    {
        Created,
        PaymentPending,
        PaymentCompleted,
        PaymentFailed,
        Shipped,
        Delivered,
        Cancelled
    }
}
