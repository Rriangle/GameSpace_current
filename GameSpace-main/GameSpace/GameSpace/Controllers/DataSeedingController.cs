using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Services;

namespace GameSpace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class DataSeedingController : ControllerBase
    {
        private readonly DataSeedingService _dataSeedingService;

        public DataSeedingController(DataSeedingService dataSeedingService)
        {
            _dataSeedingService = dataSeedingService;
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                await _dataSeedingService.SeedDataAsync();
                return Ok(new { message = "資料種子已成功完成" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "資料種子失敗", error = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetSeedingStatus()
        {
            // 這會檢查目前的資料數量
            return Ok(new { message = "資料種子狀態端點" });
        }
    }
}