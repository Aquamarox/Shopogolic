using Microsoft.EntityFrameworkCore;
using PaymentsService.Database;
using PaymentsService.Models;

namespace PaymentsService.UseCases.ProcessPayment
{
    public class ProcessPaymentService(PaymentContext context) : IProcessPaymentService
    {
        private readonly PaymentContext _context = context;

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(
            Guid orderId,
            Guid userId,
            decimal amount,
            CancellationToken cancellationToken)
        {
            // Проверяем, не обрабатывали ли мы уже этот заказ (идемпотентность)
            Transaction? existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.OrderId == orderId, cancellationToken);

            if (existingTransaction != null)
            {
                return new ProcessPaymentResult(
                    existingTransaction.Status == TransactionStatus.Completed,
                    $"Order already processed with status: {existingTransaction.Status}"
                );
            }

            Account? account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

            if (account == null)
            {
                return new ProcessPaymentResult(false, $"Account for user {userId} not found");
            }

            if (account.Balance < amount)
            {
                return new ProcessPaymentResult(false, $"Insufficient funds. Available: {account.Balance}, Required: {amount}");
            }

            // Атомарное списание с использованием Compare and Swap
            decimal oldBalance = account.Balance;
            decimal newBalance = oldBalance - amount;
            decimal newHeldAmount = account.HeldAmount;

            int updatedRows = await _context.Accounts
                .Where(a => a.Id == account.Id && a.Version == account.Version)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Balance, newBalance)
                    .SetProperty(a => a.HeldAmount, newHeldAmount)
                    .SetProperty(a => a.UpdatedAt, DateTimeOffset.UtcNow)
                    .SetProperty(a => a.Version, a => a.Version + 1),
                cancellationToken);

            if (updatedRows == 0)
            {
                return new ProcessPaymentResult(false, "Concurrent update detected, please retry");
            }

            // Создаем транзакцию
            Transaction transaction = new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                UserId = userId,
                Amount = amount,
                Type = TransactionType.Withdrawal,
                Status = TransactionStatus.Completed,
                CreatedAt = DateTimeOffset.UtcNow,
                ProcessedAt = DateTimeOffset.UtcNow
            };

            _ = await _context.Transactions.AddAsync(transaction, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new ProcessPaymentResult(true);
        }
    }

}
