using Microsoft.AspNetCore.Mvc;

namespace PaymentsService.Controllers
{
    /// <summary>
    /// Контроллер для проверки состояния здоровья (health check) сервиса платежей.
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
                Service = "PaymentsService",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
