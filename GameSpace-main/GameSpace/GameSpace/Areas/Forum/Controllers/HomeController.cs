using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.Forum.Controllers
{
    [Area("Forum")]
    [Authorize]
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

        public async Task<IActionResult> Topics()
        {
            // Get forum topics (placeholder - would need ForumTopic model)
            return View();
        }

        public async Task<IActionResult> CreateTopic()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTopic(string title, string content)
        {
            // Create new forum topic (placeholder implementation)
            return RedirectToAction(nameof(Topics));
        }
    }
}