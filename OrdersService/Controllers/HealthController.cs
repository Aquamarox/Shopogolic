using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers
{
    /// <summary>
    /// Контроллер для проверки работоспособности сервиса (Health Check).
    /// Используется Docker или Kubernetes для мониторинга состояния приложения.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "OrdersService",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
