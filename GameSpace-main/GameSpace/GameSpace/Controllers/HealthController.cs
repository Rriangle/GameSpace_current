using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;

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
            return Ok(new { status = "正常", timestamp = DateTime.UtcNow });
        }

        [HttpGet("db")]
        public async Task<IActionResult> GetDbHealth()
        {
            try
            {
                // 測試資料庫連線
                await _context.Database.CanConnectAsync();
                return Ok(new { status = "正常", database = "已連線", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "錯誤", database = "未連線", error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }
    }
}