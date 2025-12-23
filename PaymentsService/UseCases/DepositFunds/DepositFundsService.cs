using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using PaymentsService.Models;

namespace PaymentsService.UseCases.DepositFunds
{
    public class DepositFundsService(PaymentContext context) : IDepositFundsService
    {
        private readonly PaymentContext _context = context;

        public async Task<decimal> DepositAsync(Guid userId, decimal amount, CancellationToken cancellationToken)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be positive", nameof(amount));
            }

            Account account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken) ?? throw new InvalidOperationException($"Account for user {userId} not found");

            // Используем Compare and Swap для атомарного обновления
            decimal oldBalance = account.Balance;
            decimal newBalance = oldBalance + amount;

            int updatedRows = await _context.Accounts
                .Where(a => a.Id == account.Id && a.Version == account.Version)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Balance, newBalance)
                    .SetProperty(a => a.UpdatedAt, DateTimeOffset.UtcNow)
                    .SetProperty(a => a.Version, a => a.Version + 1),
                cancellationToken);

            if (updatedRows == 0)
            {
                throw new InvalidOperationException("Concurrent update detected, please retry");
            }

            // Создаем транзакцию для истории
            Transaction transaction = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = amount,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                CreatedAt = DateTimeOffset.UtcNow,
                ProcessedAt = DateTimeOffset.UtcNow
            };

            _ = await _context.Transactions.AddAsync(transaction, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return newBalance;
        }
    }

}
