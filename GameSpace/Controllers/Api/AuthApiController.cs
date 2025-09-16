using GameSpace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(IAuthService authService, ILogger<AuthApiController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(request);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = "註冊成功",
                    token = result.Token,
                    refreshToken = result.RefreshToken,
                    user = new
                    {
                        userId = result.User?.UserId,
                        userAccount = result.User?.UserAccount
                    },
                    expiresAt = result.ExpiresAt
                });
            }

            return BadRequest(new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = "登入成功",
                    token = result.Token,
                    refreshToken = result.RefreshToken,
                    user = new
                    {
                        userId = result.User?.UserId,
                        userAccount = result.User?.UserAccount
                    },
                    expiresAt = result.ExpiresAt
                });
            }

            return BadRequest(new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { success = false, message = "Refresh Token 不能為空" });
            }

            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Token 刷新成功",
                    token = result.Token,
                    refreshToken = result.RefreshToken,
                    user = new
                    {
                        userId = result.User?.UserId,
                        userAccount = result.User?.UserAccount
                    },
                    expiresAt = result.ExpiresAt
                });
            }

            return BadRequest(new
            {
                success = false,
                message = result.ErrorMessage
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var success = await _authService.LogoutAsync(request.RefreshToken);
            
            return Ok(new
            {
                success = success,
                message = success ? "登出成功" : "登出失敗"
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized(new { success = false, message = "未找到用戶資訊" });
            }

            var userId = int.Parse(userIdClaim.Value);
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new { success = false, message = "用戶不存在" });
            }

            return Ok(new
            {
                success = true,
                user = new
                {
                    userId = user.UserId,
                    userAccount = user.UserAccount,
                    createdAt = user.CreatedAt,
                    updatedAt = user.UpdatedAt
                }
            });
        }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}