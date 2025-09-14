using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GameSpace.Data;
using GameSpace.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Security.Principal;

namespace GameSpace.Tests.Infrastructure;

/// <summary>
/// 測試基礎類別，提供通用的測試設定和輔助方法
/// </summary>
public abstract class TestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;
    protected readonly GameSpaceDbContext Context;

    protected TestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = Factory.CreateClient();
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<GameSpaceDbContext>();
    }

    /// <summary>
    /// 建立測試用的 HTTP 客戶端，包含認證資訊
    /// </summary>
    /// <param name="userId">用戶 ID</param>
    /// <param name="roles">用戶角色</param>
    /// <returns>包含認證的 HTTP 客戶端</returns>
    protected HttpClient CreateAuthenticatedClient(int userId = 1, params string[] roles)
    {
        var client = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 移除真實的認證服務
                var authDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuthenticationService));
                if (authDescriptor != null)
                {
                    services.Remove(authDescriptor);
                }

                // 添加測試用的認證服務
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

                services.AddAuthorization();
            });
        }).CreateClient();

        // 設定認證標頭
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, $"TestUser{userId}")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        // 這裡可以設定認證標頭或其他認證方式
        return client;
    }

    /// <summary>
    /// 建立測試資料庫上下文（使用 InMemory 資料庫）
    /// </summary>
    /// <returns>測試用的資料庫上下文</returns>
    protected GameSpaceDbContext CreateTestDbContext()
    {
        var options = new DbContextOptionsBuilder<GameSpaceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new GameSpaceDbContext(options);
    }

    /// <summary>
    /// 清理測試資料
    /// </summary>
    protected virtual void Cleanup()
    {
        Context.Database.EnsureDeleted();
        Scope?.Dispose();
        Client?.Dispose();
    }

    public void Dispose()
    {
        Cleanup();
    }
}

/// <summary>
/// 測試用的認證處理器
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "TestUser")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}