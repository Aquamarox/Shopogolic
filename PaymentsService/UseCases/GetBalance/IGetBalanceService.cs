namespace PaymentsService.UseCases.GetBalance
{
    public interface IGetBalanceService
    {
        Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken);
    }

}
