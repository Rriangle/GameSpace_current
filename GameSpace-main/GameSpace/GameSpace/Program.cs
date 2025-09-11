// ---- Service namespaces (general using) ----
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
// ---- Type aliases (avoid duplicate interfaces/namespaces in solution, causing DI mismatch) ----
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;
using INotificationServiceAlias = GameSpace.Areas.social_hub.Services.INotificationService;
using MuteFilterAlias = GameSpace.Areas.social_hub.Services.MuteFilter;
using NotificationServiceAlias = GameSpace.Areas.social_hub.Services.NotificationService;
// ✅ Added: Permission service aliases
using IManagerPermissionServiceAlias = GameSpace.Areas.social_hub.Services.IManagerPermissionService;
using ManagerPermissionServiceAlias = GameSpace.Areas.social_hub.Services.ManagerPermissionService;

namespace GameSpace
{
	public class Program
	{
		public static async Task Main(string[] args) // ⬅ Changed to async
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
				?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// GameSpace main database (EF DbContext)
			var gameSpaceConnectionString = builder.Configuration.GetConnectionString("GameSpace")
				?? throw new InvalidOperationException("Connection string 'GameSpace' not found.");
			builder.Services.AddDbContext<GameSpacedatabaseContext>(options =>
				options.UseSqlServer(gameSpaceConnectionString));

			// ASP.NET Core Identity
			builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			// MVC
			builder.Services.AddControllersWithViews();

			// ===== social_hub related service registration =====
			builder.Services.AddMemoryCache();

			// Profanity filter options (required, MuteFilter gets through IOptions<>)
			builder.Services.Configure<MuteFilterOptions>(o =>
			{
				o.MaskStyle = MaskStyle.Asterisks;   // or FixedLabel
				o.FixedLabel = "【BLOCKED】";
				o.FuzzyBetweenCjkChars = true;       // Allow spaces/punctuation/zero-width between CJK chars
			});

			// Register with aliases to avoid namespace conflicts
			builder.Services.AddScoped<IMuteFilterAlias, MuteFilterAlias>();
			builder.Services.AddScoped<INotificationServiceAlias, NotificationServiceAlias>();
			// ✅ Added: Permission service registration (used for Mutes Create/Edit/Delete authorization)
			builder.Services.AddScoped<IManagerPermissionServiceAlias, ManagerPermissionServiceAlias>();

			// MiniGame area services
			builder.Services.AddScoped<GameSpace.Areas.MiniGame.Services.FakeDataService>();

			// SignalR
			builder.Services.AddSignalR();

			var app = builder.Build();

			// Preload dictionary on startup (failure doesn't block site)
			using (var scope = app.Services.CreateScope())
			{
				try
				{
					var filter = scope.ServiceProvider.GetRequiredService<IMuteFilterAlias>();
					await filter.RefreshAsync(); // Warm up Regex and cache
												 // If you don't want to change Main to async, change to: filter.RefreshAsync().GetAwaiter().GetResult();
				}
				catch { /* ignore */ }
			}

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication(); // Identity
			app.UseAuthorization();

			// Add area routes first (important)
			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			// Then add general default routes
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.MapRazorPages();

			// Register SignalR hub (use full type name to avoid namespace conflicts)
			app.MapHub<GameSpace.Areas.social_hub.Hubs.ChatHub>("/social_hub/chatHub");

			await app.RunAsync(); // ⬅ Matches async Main
		}
	}
}