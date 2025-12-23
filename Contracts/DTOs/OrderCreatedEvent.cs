namespace Contracts.DTOs
{
    /// <summary>
    /// Событие, возникающее при создании нового заказа. 
    /// Служит сигналом для сервиса платежей о необходимости списания средств.
    /// </summary>
    /// <param name="OrderId">Идентификатор созданного заказа.</param>
    /// <param name="UserId">Идентификатор пользователя, совершившего заказ.</param>
    /// <param name="TotalAmount">Общая сумма заказа к оплате.</param>
    /// <param name="Items">Список позиций, входящих в заказ.</param>
    /// <param name="CreatedAt">Дата и время создания заказа.</param>
    public sealed record OrderCreatedEvent(
        Guid OrderId,
        Guid UserId,
        decimal TotalAmount,
        List<OrderItemDto> Items,
        DateTimeOffset CreatedAt);
}
