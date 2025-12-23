namespace PaymentsService.Common.SendPaymentResult
{
    public interface ISendPaymentResultService
    {
        Task SendPaymentResultAsync(
            Guid orderId,
            Guid userId,
            decimal amount,
            bool success,
            string? reason,
            CancellationToken cancellationToken);
    }

}
