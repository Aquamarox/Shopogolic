using OrdersService.Database;
using Microsoft.EntityFrameworkCore;
using OrdersService.Models;

namespace OrdersService.UseCases.GetOrders
{
    public class GetOrdersService(OrderContext context) : IGetOrdersService
    {
        private readonly OrderContext _context = context;

        public async Task<List<Order>> GetOrdersAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
