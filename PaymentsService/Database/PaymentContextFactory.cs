using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace PaymentsService.Database
{
    /// <summary>
    /// Настройка конфигурации моделей и связей в базе данных при её создании.
    /// Включает описание ограничений, индексов для идемпотентности и типов колонок.
    /// </summary>
    public class PaymentContextFactory : IDesignTimeDbContextFactory<PaymentContext>
    {
        public PaymentContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<PaymentContext> optionsBuilder = new();

            string connectionString = "Host=localhost;Port=5433;Database=paymentsdb;Username=paymentsuser;Password=paymentspass";

            _ = optionsBuilder.UseNpgsql(connectionString);

            return new PaymentContext(optionsBuilder.Options);
        }
    }

}
