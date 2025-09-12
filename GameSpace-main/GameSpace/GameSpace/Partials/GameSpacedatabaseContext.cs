using Microsoft.EntityFrameworkCore;

namespace GameSpace.Models
{

	public partial class GameSpacedatabaseContext : DbContext
	{
		public GameSpacedatabaseContext() { 
		}

		// DbSet properties for all models
		public DbSet<User> Users { get; set; }
		public DbSet<Pet> Pet { get; set; }
		public DbSet<User_Wallet> User_Wallet { get; set; }
		public DbSet<UserSignInStat> UserSignInStat { get; set; }
		public DbSet<MiniGame> MiniGame { get; set; }
		public DbSet<WalletHistory> WalletHistory { get; set; }
		public DbSet<Coupon> Coupon { get; set; }
		public DbSet<CouponType> CouponType { get; set; }
		public DbSet<Evoucher> Evoucher { get; set; }
		public DbSet<EvoucherType> EvoucherType { get; set; }
		public DbSet<EvoucherToken> EvoucherToken { get; set; }
		public DbSet<EvoucherRedeemLog> EvoucherRedeemLog { get; set; }
		public DbSet<UserIntroduce> UserIntroduce { get; set; }
		public DbSet<UserRight> UserRight { get; set; }
		public DbSet<UserToken> UserToken { get; set; }
		public DbSet<UserSalesInformation> UserSalesInformation { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{ 
			IConfigurationRoot Configuration =
					new ConfigurationBuilder()
					.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
					.AddJsonFile("appsettings.json")
					.Build();
				optionsBuilder.UseSqlServer(Configuration.GetConnectionString("GameSpace"));
			}
		}
	}
}
