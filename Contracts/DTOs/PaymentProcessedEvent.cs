using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.DTOs
{
    public sealed record PaymentProcessedEvent(
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    DateTimeOffset ProcessedAt);
}
