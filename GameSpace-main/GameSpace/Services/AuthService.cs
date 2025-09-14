using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Services
{
    public class AuthService : IAuthService
    {
        private readonly GameSpaceDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(GameSpaceDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // 檢查帳號是否已存在
                if (await _context.Users.AnyAsync(u => u.UserAccount == request.UserAccount))
                {
                    return new AuthResult { Success = false, ErrorMessage = "帳號已存在" };
                }

                // 檢查 Email 是否已存在
                if (await _context.UserIntroduces.AnyAsync(ui => ui.Email == request.Email))
                {
                    return new AuthResult { Success = false, ErrorMessage = "Email 已存在" };
                }

                // 檢查手機號碼是否已存在
                if (await _context.UserIntroduces.AnyAsync(ui => ui.Cellphone == request.Cellphone))
                {
                    return new AuthResult { Success = false, ErrorMessage = "手機號碼已存在" };
                }

                // 檢查身分證字號是否已存在
                if (await _context.UserIntroduces.AnyAsync(ui => ui.IdNumber == request.IdNumber))
                {
                    return new AuthResult { Success = false, ErrorMessage = "身分證字號已存在" };
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 建立用戶主檔
                    var user = new User
                    {
                        UserAccount = request.UserAccount,
                        UserPassword = HashPassword(request.UserPassword),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // 建立用戶詳細資料
                    var userIntroduce = new UserIntroduce
                    {
                        UserId = user.UserId,
                        UserNickName = request.UserNickName,
                        Gender = request.Gender,
                        IdNumber = request.IdNumber,
                        Cellphone = request.Cellphone,
                        Email = request.Email,
                        Address = request.Address,
                        DateOfBirth = request.DateOfBirth,
                        CreateAccount = DateTime.UtcNow,
                        UserIntroduceText = "歡迎來到 GameSpace！"
                    };

                    _context.UserIntroduces.Add(userIntroduce);

                    // 建立用戶權限
                    var userRights = new UserRight
                    {
                        UserId = user.UserId,
                        UserStatus = true,
                        ShoppingPermission = true,
                        MessagePermission = true,
                        SalesAuthority = false
                    };

                    _context.UserRights.Add(userRights);

                    // 建立用戶錢包
                    var userWallet = new UserWallet
                    {
                        UserId = user.UserId,
                        UserPoint = 1000, // 註冊獎勵 1000 點
                        CouponNumber = "0",
                        EVoucherNumber = "0"
                    };

                    _context.UserWallets.Add(userWallet);

                    // 建立寵物
                    var pet = new Pet
                    {
                        UserId = user.UserId,
                        PetName = "小可愛",
                        Level = 1,
                        LevelUpTime = DateTime.UtcNow,
                        Experience = 0,
                        Hunger = 50,
                        Mood = 50,
                        Stamina = 50,
                        Cleanliness = 50,
                        Health = 100,
                        SkinColor = "#ADD8E6",
                        SkinColorChangedTime = DateTime.UtcNow,
                        BackgroundColor = "粉藍",
                        BackgroundColorChangedTime = DateTime.UtcNow,
                        PointsChangedColor = 0,
                        PointsChangedTimeSkinColor = DateTime.UtcNow,
                        PointsChangedBackgroundColor = 0,
                        PointsGainedLevelUp = 0,
                        PointsGainedTimeLevelUp = DateTime.UtcNow
                    };

                    _context.Pets.Add(pet);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 生成 JWT Token
                    var token = await GenerateJwtTokenAsync(user);
                    var refreshToken = await GenerateRefreshTokenAsync();

                    // 儲存 Refresh Token
                    var userToken = new UserToken
                    {
                        UserId = user.UserId,
                        Token = refreshToken,
                        ExpiresAt = DateTime.UtcNow.AddDays(30),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserTokens.Add(userToken);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("用戶註冊成功: {UserAccount}", request.UserAccount);

                    return new AuthResult
                    {
                        Success = true,
                        Token = token,
                        RefreshToken = refreshToken,
                        User = user,
                        ExpiresAt = DateTime.UtcNow.AddHours(24)
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用戶註冊失敗: {UserAccount}", request.UserAccount);
                return new AuthResult { Success = false, ErrorMessage = "註冊失敗，請稍後再試" };
            }
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            try
            {
                // 查找用戶
                var user = await _context.Users
                    .Include(u => u.UserIntroduce)
                    .Include(u => u.UserRights)
                    .FirstOrDefaultAsync(u => u.UserAccount == request.Account);

                if (user == null)
                {
                    return new AuthResult { Success = false, ErrorMessage = "帳號或密碼錯誤" };
                }

                // 檢查用戶狀態
                if (!user.UserRights?.UserStatus ?? true)
                {
                    return new AuthResult { Success = false, ErrorMessage = "帳號已被停權" };
                }

                // 驗證密碼
                if (!await ValidatePasswordAsync(user, request.Password))
                {
                    return new AuthResult { Success = false, ErrorMessage = "帳號或密碼錯誤" };
                }

                // 生成 JWT Token
                var token = await GenerateJwtTokenAsync(user);
                var refreshToken = await GenerateRefreshTokenAsync();

                // 儲存 Refresh Token
                var userToken = new UserToken
                {
                    UserId = user.UserId,
                    Token = refreshToken,
                    ExpiresAt = request.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserTokens.Add(userToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation("用戶登入成功: {UserAccount}", request.Account);

                return new AuthResult
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    User = user,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用戶登入失敗: {Account}", request.Account);
                return new AuthResult { Success = false, ErrorMessage = "登入失敗，請稍後再試" };
            }
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var userToken = await _context.UserTokens
                    .Include(ut => ut.User)
                    .FirstOrDefaultAsync(ut => ut.Token == refreshToken && ut.ExpiresAt > DateTime.UtcNow);

                if (userToken == null)
                {
                    return new AuthResult { Success = false, ErrorMessage = "無效的 Refresh Token" };
                }

                // 生成新的 JWT Token
                var newToken = await GenerateJwtTokenAsync(userToken.User!);
                var newRefreshToken = await GenerateRefreshTokenAsync();

                // 更新 Refresh Token
                userToken.Token = newRefreshToken;
                userToken.ExpiresAt = DateTime.UtcNow.AddDays(30);
                userToken.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new AuthResult
                {
                    Success = true,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    User = userToken.User,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh Token 失敗: {RefreshToken}", refreshToken);
                return new AuthResult { Success = false, ErrorMessage = "Token 刷新失敗" };
            }
        }

        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                var userToken = await _context.UserTokens
                    .FirstOrDefaultAsync(ut => ut.Token == token);

                if (userToken != null)
                {
                    _context.UserTokens.Remove(userToken);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用戶登出失敗: {Token}", token);
                return false;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.UserIntroduce)
                .Include(u => u.UserRights)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserByAccountAsync(string account)
        {
            return await _context.Users
                .Include(u => u.UserIntroduce)
                .Include(u => u.UserRights)
                .FirstOrDefaultAsync(u => u.UserAccount == account);
        }

        public async Task<bool> ValidatePasswordAsync(User user, string password)
        {
            return user.UserPassword == HashPassword(password);
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserAccount),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("UserAccount", user.UserAccount)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int?> GetUserIdFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserId");
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
            }
            catch
            {
                return null;
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + _configuration["Jwt:Salt"]));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}