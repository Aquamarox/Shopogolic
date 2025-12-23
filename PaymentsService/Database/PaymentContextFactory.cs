using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace PaymentsService.Database
{
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
