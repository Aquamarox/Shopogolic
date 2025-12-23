using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.DTOs
{
    /// <summary>
    /// Событие успешного завершения транзакции по заказу.
    /// </summary>
    /// <param name="OrderId">Идентификатор оплаченного заказа.</param>
    /// <param name="UserId">Идентификатор владельца счета.</param>
    /// <param name="Amount">Списанная сумма.</param>
    /// <param name="ProcessedAt">Дата и время успешной обработки платежа.</param>
    public sealed record PaymentFailedEvent(
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    string Reason,
    DateTimeOffset FailedAt);
}
