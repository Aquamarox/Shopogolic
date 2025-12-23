using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.DTOs
{
    public sealed record PaymentFailedEvent(
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    string Reason,
    DateTimeOffset FailedAt);
}
