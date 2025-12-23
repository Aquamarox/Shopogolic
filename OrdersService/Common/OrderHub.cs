using Microsoft.AspNetCore.SignalR;

namespace OrdersService.Common
{
    /// <summary>
    /// SignalR хаб для передачи обновлений о статусе заказа клиенту в реальном времени.
    /// </summary>
    public class OrderHub : Hub
    {
        /// <summary>
        /// Подписывает клиента на группу уведомлений по его UserId.
        /// Это позволяет отправлять сообщения только конкретному пользователю.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя для создания группы.</param>
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}