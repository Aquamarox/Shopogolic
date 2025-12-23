namespace PaymentsService.UseCases.ProcessPayment
{
    public interface IProcessPaymentService
    {
        Task<ProcessPaymentResult> ProcessPaymentAsync(
            Guid orderId,
            Guid userId,
            decimal amount,
            CancellationToken cancellationToken);
    }

}
