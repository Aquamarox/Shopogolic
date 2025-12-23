using Microsoft.AspNetCore.Mvc;
using OrdersService.Models;
using OrdersService.UseCases.CreateOrder;
using OrdersService.UseCases.GetOrders;
using OrdersService.UseCases.GetOrderStatus;

namespace OrdersService.Controllers
{

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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            CreateOrderResponse response = await _createOrderService.CreateOrderAsync(request, HttpContext.RequestAborted);
            return Ok(response);
        }

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
