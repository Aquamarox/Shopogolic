using Microsoft.EntityFrameworkCore;

namespace OrdersService.Database
{
    public class MigrationRunner(IServiceProvider serviceProvider) : IHostedService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            OrderContext context = scope.ServiceProvider.GetRequiredService<OrderContext>();

            await context.Database.MigrateAsync(cancellationToken);
            Console.WriteLine("Migrations applied successfully for OrdersService");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}
