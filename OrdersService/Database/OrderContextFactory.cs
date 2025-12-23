using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrdersService.Database
{
    public class OrderContextFactory : IDesignTimeDbContextFactory<OrderContext>
    {
        public OrderContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<OrderContext> optionsBuilder = new();
            _ = optionsBuilder.UseNpgsql("Host=localhost;Database=ordersdb;Username=ordersuser;Password=orderspass");

            return new OrderContext(optionsBuilder.Options);
        }
    }
}
