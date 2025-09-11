using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Services;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminController : Controller
    {
        private readonly FakeDataService _fakeDataService;

        public AdminController(FakeDataService fakeDataService)
        {
            _fakeDataService = fakeDataService;
        }

        [HttpPost("generate-fake-data")]
        public async Task<IActionResult> GenerateFakeData()
        {
            try
            {
                await _fakeDataService.GenerateFakeDataAsync();
                return Ok(new { message = "Fake data generated successfully", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }
    }
}