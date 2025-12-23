namespace OrdersService.Models
{
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
