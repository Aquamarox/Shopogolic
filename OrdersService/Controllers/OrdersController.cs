using Microsoft.AspNetCore.Mvc;
using OrdersService.Models;
using OrdersService.UseCases.CreateOrder;
using OrdersService.UseCases.GetOrders;
using OrdersService.UseCases.GetOrderStatus;

namespace OrdersService.Controllers
{
    /// <summary>
    /// Контроллер для управления заказами.
    /// Предоставляет методы для создания заказов и получения их статуса.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController(
        ICreateOrderService createOrderService,
        IGetOrdersService getOrdersService,
        IGetOrderStatusService getOrderStatusService) : ControllerBase
    {
        private readonly ICreateOrderService _createOrderService = createOrderService;
        private readonly IGetOrdersService _getOrdersService = getOrdersService;
        private readonly IGetOrderStatusService _getOrderStatusService = getOrderStatusService;

        /// <summary>
        /// Создает новый заказ в системе.
        /// </summary>
        /// <param name="request">Данные для создания заказа (ID пользователя, список товаров).</param>
        /// <returns>Результат создания заказа с его идентификатором.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            CreateOrderResponse response = await _createOrderService.CreateOrderAsync(request, HttpContext.RequestAborted);
            return Ok(response);
        }

        /// <summary>
        /// Получает список всех заказов конкретного пользователя.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] Guid userId)
        {
            List<Order> orders = await _getOrdersService.GetOrdersAsync(userId, HttpContext.RequestAborted);
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderStatus(
            [FromRoute] Guid orderId,
            [FromQuery] Guid userId)
        {
            Order? order = await _getOrderStatusService.GetOrderStatusAsync(orderId, userId, HttpContext.RequestAborted);

            return order == null ? NotFound($"Order {orderId} not found for user {userId}") : Ok(order);
        }
    }
}
