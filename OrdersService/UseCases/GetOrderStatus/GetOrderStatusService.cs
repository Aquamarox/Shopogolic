using OrdersService.Database;
using Microsoft.EntityFrameworkCore;
using OrdersService.Models;

namespace OrdersService.UseCases.GetOrderStatus
{
    public class GetOrderStatusService(OrderContext context) : IGetOrderStatusService
    {
        private readonly OrderContext _context = context;

        public async Task<Order?> GetOrderStatusAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, cancellationToken);
        }
    }

}
