using GameSpace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.Public.Controllers
{
    [Area("Public")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _authService.LoginAsync(request);

            if (result.Success)
            {
                // 儲存 JWT Token 到 Cookie
                Response.Cookies.Append("jwt_token", result.Token!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.ExpiresAt
                });

                // 儲存 Refresh Token 到 Cookie
                Response.Cookies.Append("refresh_token", result.RefreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30)
                });

                TempData["SuccessMessage"] = "登入成功！";
                return RedirectToAction("Index", "Home", new { area = "MiniGame" });
            }

            ModelState.AddModelError("", result.ErrorMessage ?? "登入失敗");
            return View(request);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var result = await _authService.RegisterAsync(request);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "註冊成功！歡迎來到 GameSpace！";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", result.ErrorMessage ?? "註冊失敗");
            return View(request);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
            }

            Response.Cookies.Delete("jwt_token");
            Response.Cookies.Delete("refresh_token");

            TempData["SuccessMessage"] = "已成功登出";
            return RedirectToAction("Index", "Home", new { area = "Public" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User user)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", user);
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null || currentUserId != user.UserId)
            {
                return RedirectToAction("Login");
            }

            // 更新用戶資料的邏輯
            // 這裡可以實作更新用戶資料的功能

            TempData["SuccessMessage"] = "個人資料更新成功！";
            return RedirectToAction("Profile");
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
        }
    }
}