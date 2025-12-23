using Microsoft.AspNetCore.Mvc;

namespace PaymentsService.Controllers
{
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
