using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;

namespace GameSpace.Areas.Public.Controllers
{
    [Area("Public")]
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