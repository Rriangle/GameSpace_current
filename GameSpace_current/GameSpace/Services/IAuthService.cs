using GameSpace.Models;

namespace GameSpace.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string token);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByAccountAsync(string account);
        Task<bool> ValidatePasswordAsync(User user, string password);
        Task<string> GenerateJwtTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> IsTokenValidAsync(string token);
        Task<int?> GetUserIdFromTokenAsync(string token);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public User? User { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class RegisterRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public string UserPassword { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cellphone { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string UserNickName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
    }

    public class LoginRequest
    {
        public string Account { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }
}