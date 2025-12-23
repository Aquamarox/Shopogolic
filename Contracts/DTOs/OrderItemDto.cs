namespace Contracts.DTOs
{
    /// <summary>
    /// Данные о конкретной позиции в заказе.
    /// </summary>
    /// <param name="ProductId">Уникальный идентификатор товара.</param>
    /// <param name="ProductName">Наименование товара.</param>
    /// <param name="Quantity">Количество единиц товара.</param>
    /// <param name="Price">Цена за единицу товара.</param>
    public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Price);
}
