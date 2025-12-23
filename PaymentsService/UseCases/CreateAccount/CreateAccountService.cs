using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using PaymentsService.Models;

namespace PaymentsService.UseCases.CreateAccount
{
    public class CreateAccountService(PaymentContext context) : ICreateAccountService
    {
        private readonly PaymentContext _context = context;

        public async Task<Guid> CreateAccountAsync(Guid userId, CancellationToken cancellationToken)
        {
            // Проверяем, есть ли уже счет у пользователя
            Account? existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

            if (existingAccount != null)
            {
                return existingAccount.Id;
            }

            // Создаем новый счет
            Account account = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Balance = 0,
                HeldAmount = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Version = 1
            };

            _ = await _context.Accounts.AddAsync(account, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return account.Id;
        }
    }

}
