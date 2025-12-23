namespace PaymentsService.UseCases.CreateAccount
{
    public interface ICreateAccountService
    {
        Task<Guid> CreateAccountAsync(Guid userId, CancellationToken cancellationToken);
    }
}
