using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using System.Net.Http.Json;
using FluentAssertions;
using System.Text.Json;

namespace GameSpace.Tests.E2E;

/// <summary>
/// 用戶旅程端對端測試
/// </summary>
public class UserJourneyTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly GameSpaceDbContext _context;

    public UserJourneyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 移除現有的 DbContext
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<GameSpaceDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 添加 In-Memory 資料庫
                services.AddDbContext<GameSpaceDbContext>(options =>
                {
                    options.UseInMemoryDatabase("E2ETestDatabase_" + Guid.NewGuid().ToString());
                });
            });
        });

        _client = _factory.CreateClient();
        _context = _factory.Services.GetRequiredService<GameSpaceDbContext>();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task 完整用戶註冊到寵物養成旅程_應該成功完成()
    {
        // 1. 用戶註冊
        var registerRequest = new
        {
            User_Account = "testuser",
            User_Password = "Password123!",
            User_Email = "test@example.com",
            User_Phone = "0912345678",
            User_IDNumber = "A123456789",
            User_Nickname = "測試用戶",
            User_BirthDate = DateTime.Now.AddYears(-20)
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // 2. 用戶登入
        var loginRequest = new
        {
            User_Account = "testuser",
            User_Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<Dictionary<string, object>>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var token = loginResult["token"].ToString();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 3. 創建寵物
        var createPetRequest = new { petName = "我的史萊姆" };
        var createPetResponse = await _client.PostAsJsonAsync("/api/pet/1", createPetRequest);
        createPetResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // 4. 查看寵物狀態
        var getPetResponse = await _client.GetAsync("/api/pet/1");
        getPetResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var petContent = await getPetResponse.Content.ReadAsStringAsync();
        var pet = JsonSerializer.Deserialize<Pet>(petContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        pet.Should().NotBeNull();
        pet.Pet_Name.Should().Be("我的史萊姆");
        pet.Level.Should().Be(1);

        // 5. 與寵物互動（餵食）
        var interactRequest = new { action = "餵食" };
        var interactResponse = await _client.PostAsJsonAsync("/api/pet/1/interact", interactRequest);
        interactResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 6. 查看寵物更新後的狀態
        var updatedPetResponse = await _client.GetAsync("/api/pet/1");
        updatedPetResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 7. 每日簽到
        var signInResponse = await _client.PostAsJsonAsync("/api/signin/1", new { });
        signInResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 8. 查看錢包餘額
        var walletResponse = await _client.GetAsync("/api/wallet/1");
        walletResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var walletContent = await walletResponse.Content.ReadAsStringAsync();
        var wallet = JsonSerializer.Deserialize<Dictionary<string, object>>(walletContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        wallet.Should().NotBeNull();
        wallet["userPoint"].Should().NotBeNull();
    }

    [Fact]
    public async Task 商城購物到訂單完成旅程_應該成功完成()
    {
        // 1. 用戶登入（使用已存在的測試用戶）
        var loginRequest = new
        {
            User_Account = "testuser",
            User_Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<Dictionary<string, object>>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var token = loginResult["token"].ToString();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. 查看商品列表
        var productsResponse = await _client.GetAsync("/api/shop/products");
        productsResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 3. 添加商品到購物車
        var addToCartRequest = new
        {
            ProductId = 1,
            Quantity = 2
        };

        var addToCartResponse = await _client.PostAsJsonAsync("/api/shop/cart/add", addToCartRequest);
        addToCartResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 4. 查看購物車
        var cartResponse = await _client.GetAsync("/api/shop/cart");
        cartResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 5. 結帳
        var checkoutRequest = new
        {
            PaymentMethod = "credit_card",
            ShippingAddress = "台北市信義區信義路五段7號"
        };

        var checkoutResponse = await _client.PostAsJsonAsync("/api/shop/checkout", checkoutRequest);
        checkoutResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 6. 查看訂單歷史
        var ordersResponse = await _client.GetAsync("/api/shop/orders");
        ordersResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task 論壇發文到社群互動旅程_應該成功完成()
    {
        // 1. 用戶登入
        var loginRequest = new
        {
            User_Account = "testuser",
            User_Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<Dictionary<string, object>>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var token = loginResult["token"].ToString();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. 發表文章
        var createPostRequest = new
        {
            Title = "測試文章標題",
            Content = "這是一篇測試文章內容",
            Category = "遊戲討論"
        };

        var createPostResponse = await _client.PostAsJsonAsync("/api/forum/posts", createPostRequest);
        createPostResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // 3. 查看文章列表
        var postsResponse = await _client.GetAsync("/api/forum/posts");
        postsResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 4. 對文章按讚
        var likeRequest = new { PostId = 1 };
        var likeResponse = await _client.PostAsJsonAsync("/api/forum/posts/1/like", likeRequest);
        likeResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 5. 發表評論
        var commentRequest = new
        {
            Content = "這是一條測試評論"
        };

        var commentResponse = await _client.PostAsJsonAsync("/api/forum/posts/1/comments", commentRequest);
        commentResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // 6. 查看評論
        var commentsResponse = await _client.GetAsync("/api/forum/posts/1/comments");
        commentsResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task 小遊戲冒險到排行榜旅程_應該成功完成()
    {
        // 1. 用戶登入
        var loginRequest = new
        {
            User_Account = "testuser",
            User_Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<Dictionary<string, object>>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var token = loginResult["token"].ToString();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. 創建寵物（如果還沒有）
        var createPetRequest = new { petName = "冒險史萊姆" };
        var createPetResponse = await _client.PostAsJsonAsync("/api/pet/1", createPetRequest);
        createPetResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // 3. 開始小遊戲冒險
        var startGameRequest = new { PetId = 1 };
        var startGameResponse = await _client.PostAsJsonAsync("/api/minigame/start", startGameRequest);
        startGameResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 4. 查看遊戲結果
        var gameResultResponse = await _client.GetAsync("/api/minigame/result/1");
        gameResultResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 5. 查看排行榜
        var leaderboardResponse = await _client.GetAsync("/api/minigame/leaderboard");
        leaderboardResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 6. 查看個人統計
        var statsResponse = await _client.GetAsync("/api/minigame/stats/1");
        statsResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task 管理員後台管理旅程_應該成功完成()
    {
        // 1. 管理員登入
        var adminLoginRequest = new
        {
            Manager_Account = "admin",
            Manager_Password = "AdminPassword123!"
        };

        var adminLoginResponse = await _client.PostAsJsonAsync("/api/admin/login", adminLoginRequest);
        adminLoginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var adminLoginContent = await adminLoginResponse.Content.ReadAsStringAsync();
        var adminLoginResult = JsonSerializer.Deserialize<Dictionary<string, object>>(adminLoginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var adminToken = adminLoginResult["token"].ToString();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // 2. 查看管理員儀表板
        var dashboardResponse = await _client.GetAsync("/api/admin/dashboard");
        dashboardResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 3. 查看用戶列表
        var usersResponse = await _client.GetAsync("/api/admin/users");
        usersResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 4. 查看論壇管理
        var forumManagementResponse = await _client.GetAsync("/api/admin/forum");
        forumManagementResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 5. 查看訂單管理
        var ordersManagementResponse = await _client.GetAsync("/api/admin/orders");
        ordersManagementResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // 6. 查看系統統計
        var statsResponse = await _client.GetAsync("/api/admin/statistics");
        statsResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}