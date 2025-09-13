using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class HomeController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public HomeController(GameSpaceDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalPets = await _context.Pets.CountAsync(),
                TotalMiniGames = await _context.MiniGames.CountAsync(),
                TotalCoupons = await _context.Coupons.CountAsync(),
                TotalEVouchers = await _context.EVouchers.CountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}