using GameSpace.Data;
using GameSpace.Services;
using GameSpace.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using StackExchange.Redis;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// 設定 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/gamespace-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 加入服務
builder.Services.AddDbContext<GameSpaceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<DataSeedingService>();

// 快取服務配置
builder.Services.AddMemoryCache();

// Redis 快取服務（可選）
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(redisConnectionString));
    builder.Services.AddScoped<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddScoped<ICacheService, CacheService>();
}

// 註冊業務服務
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ISignInService, SignInService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IForumService, ForumService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 註冊效能監控服務
builder.Services.AddScoped<IPerformanceService, PerformanceService>();

// 註冊安全性服務
builder.Services.AddScoped<ISecurityService, SecurityService>();

// 設定資料保護
builder.Services.AddDataProtection()
    .SetApplicationName("GameSpace")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// 設定 Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// 設定 Cookie 安全性
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always;
});

// 設定 JWT 認證
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 設定管道
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 安全性中間件
app.UseSecurityHeaders();
app.UseInputValidation();
app.UseRateLimiting();
app.UseCsrfProtection();

app.UseSession();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseJwtMiddleware();
app.UsePerformanceMonitoring();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 資料庫種子服務
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GameSpaceDbContext>();
    var seedingService = scope.ServiceProvider.GetRequiredService<DataSeedingService>();
    
    try
    {
        await context.Database.EnsureCreatedAsync();
        await seedingService.SeedAllDataAsync();
        Log.Information("資料庫種子服務執行完成");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "資料庫種子服務執行失敗");
    }
}

app.Run();