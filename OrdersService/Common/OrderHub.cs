using Microsoft.AspNetCore.SignalR;

namespace OrdersService.Common
{
    public class OrderHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}