using Microsoft.AspNetCore.Mvc;

namespace Autouchet_Bot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        /// GET: api/data/statuss данные бэка
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                status = "Active",
                serverTime = DateTime.UtcNow
            });
        }
    }
}