using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.Public.Controllers
{
    [Area("Public")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}