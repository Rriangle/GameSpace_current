using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly GameSpacedatabaseContext _context;

        public HealthController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
        }

        [HttpGet("db")]
        public async Task<IActionResult> GetDatabaseHealth()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return Ok(new { status = "ok", database = "connected", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", database = "disconnected", error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }
    }
}