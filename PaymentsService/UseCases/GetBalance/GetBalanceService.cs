using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;

namespace PaymentsService.UseCases.GetBalance
{
    public class GetBalanceService(PaymentContext context) : IGetBalanceService
    {
        private readonly PaymentContext _context = context;

        public async Task<decimal> GetBalanceAsync(Guid userId, CancellationToken cancellationToken)
        {
            Models.Account? account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

            return account == null ? throw new InvalidOperationException($"Account for user {userId} not found") : account.Balance;
        }
    }

}
