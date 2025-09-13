using GameSpace.Data;
using GameSpace.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

// 註冊業務服務
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ISignInService, SignInService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IForumService, ForumService>();

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
app.UseRouting();
app.UseAuthorization();

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