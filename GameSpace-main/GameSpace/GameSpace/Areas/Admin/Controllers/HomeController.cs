using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Data;

namespace GameSpace.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public HomeController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            // Get dashboard statistics
            var userCount = await _context.Users.CountAsync();
            var gameCount = await _context.MiniGame.CountAsync();
            var walletCount = await _context.User_Wallet.CountAsync();

            ViewBag.UserCount = userCount;
            ViewBag.GameCount = gameCount;
            ViewBag.WalletCount = walletCount;

            return View();
        }
    }
}