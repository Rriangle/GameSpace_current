using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 開發環境專用的管理員登入控制器
    /// 僅在 Development 環境下可用，用於本機測試 Admin 功能
    /// </summary>
    [Area("MiniGame")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DevAuthController : Controller
    {
        private readonly IHostEnvironment _environment;
        private readonly ILogger<DevAuthController> _logger;

        public DevAuthController(IHostEnvironment environment, ILogger<DevAuthController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// 開發環境管理員登入
        /// GET /MiniGame/DevAuth/LoginAsManager?managerId=30000001
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> LoginAsManager(int managerId = 30000001)
        {
            // 僅在開發環境可用
            if (!_environment.IsDevelopment())
            {
                _logger.LogWarning("DevAuth: 非開發環境嘗試存取 DevAuth.LoginAsManager");
                return Forbid("此功能僅在開發環境可用");
            }

            try
            {
                // 建立管理員 Claims
                var claims = new List<Claim>
                {
                    new Claim("ManagerId", managerId.ToString()),
                    new Claim("Manager_Id", managerId.ToString()),
                    new Claim("UserId", managerId.ToString()),
                    new Claim("UserID", managerId.ToString()),
                    new Claim("sub", managerId.ToString()),
                    new Claim("id", managerId.ToString()),
                    new Claim("role", "DevAdmin"),
                    new Claim("environment", "Development")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                    IssuedUtc = DateTimeOffset.UtcNow
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("DevAuth: 管理員登入成功 - ManagerId={ManagerId}", managerId);

                return Json(new
                {
                    success = true,
                    message = $"開發環境管理員登入成功 (ManagerId: {managerId})",
                    managerId = managerId,
                    redirectUrl = "/MiniGame/AdminHome/Dashboard"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DevAuth: 管理員登入失敗 - ManagerId={ManagerId}", managerId);
                return Json(new
                {
                    success = false,
                    message = "登入失敗: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 開發環境登出
        /// GET /MiniGame/DevAuth/Logout
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // 僅在開發環境可用
            if (!_environment.IsDevelopment())
            {
                return Forbid("此功能僅在開發環境可用");
            }

            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("DevAuth: 管理員登出成功");

                return Json(new
                {
                    success = true,
                    message = "登出成功",
                    redirectUrl = "/MiniGame/Home/Index"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DevAuth: 登出失敗");
                return Json(new
                {
                    success = false,
                    message = "登出失敗: " + ex.Message
                });
            }
        }

        /// <summary>
        /// 檢查當前登入狀態
        /// GET /MiniGame/DevAuth/Status
        /// </summary>
        [HttpGet]
        public IActionResult Status()
        {
            if (!_environment.IsDevelopment())
            {
                return Forbid("此功能僅在開發環境可用");
            }

            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            var managerId = User?.FindFirst("ManagerId")?.Value;

            return Json(new
            {
                environment = _environment.EnvironmentName,
                isAuthenticated = isAuthenticated,
                managerId = managerId,
                claims = User?.Claims?.Select(c => new { c.Type, c.Value }).ToList()
            });
        }
    }
}