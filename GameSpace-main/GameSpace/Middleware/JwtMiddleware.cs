using GameSpace.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GameSpace.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            var token = context.Request.Cookies["jwt_token"];

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    if (await authService.IsTokenValidAsync(token))
                    {
                        var userId = await authService.GetUserIdFromTokenAsync(token);
                        if (userId.HasValue)
                        {
                            var user = await authService.GetUserByIdAsync(userId.Value);
                            if (user != null)
                            {
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                                    new Claim(ClaimTypes.Name, user.UserAccount),
                                    new Claim("UserId", user.UserId.ToString()),
                                    new Claim("UserAccount", user.UserAccount)
                                };

                                var identity = new ClaimsIdentity(claims, "jwt");
                                context.User = new ClaimsPrincipal(identity);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "JWT Token 驗證失敗");
                }
            }

            await _next(context);
        }
    }

    public static class JwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtMiddleware>();
        }
    }
}