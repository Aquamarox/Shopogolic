namespace PaymentsService.UseCases.DepositFunds
{
    public interface IDepositFundsService
    {
        Task<decimal> DepositAsync(Guid userId, decimal amount, CancellationToken cancellationToken);
    }
}
