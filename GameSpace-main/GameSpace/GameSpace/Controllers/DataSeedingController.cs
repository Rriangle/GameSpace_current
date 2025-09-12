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
                return Ok(new { message = "Data seeding completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Data seeding failed", error = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetSeedingStatus()
        {
            // This would check the current data counts
            return Ok(new { message = "Data seeding status endpoint" });
        }
    }
}