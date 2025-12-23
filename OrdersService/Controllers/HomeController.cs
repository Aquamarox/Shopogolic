using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers
{
    /// <summary>
    /// Контроллер для проверки работоспособности сервиса (Health Check).
    /// Используется Docker или Kubernetes для мониторинга состояния приложения.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { Status = "Healthy", Service = "OrdersService" });
        }
    }
}
