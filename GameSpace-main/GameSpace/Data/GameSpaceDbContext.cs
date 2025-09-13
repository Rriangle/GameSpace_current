using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Data
{
    public class GameSpaceDbContext : DbContext
    {
        public GameSpaceDbContext(DbContextOptions<GameSpaceDbContext> options) : base(options)
        {
        }

        // 用戶相關
        public DbSet<User> Users { get; set; }
        public DbSet<UserIntroduce> UserIntroduces { get; set; }
        public DbSet<UserRights> UserRights { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<UserSignInStats> UserSignInStats { get; set; }
        public DbSet<UserTokens> UserTokens { get; set; }
        public DbSet<UserSalesInformation> UserSalesInformations { get; set; }

        // 寵物相關
        public DbSet<Pet> Pets { get; set; }

        // 小遊戲相關
        public DbSet<MiniGame> MiniGames { get; set; }

        // 優惠券和禮券相關
        public DbSet<CouponType> CouponTypes { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<EVoucherType> EVoucherTypes { get; set; }
        public DbSet<EVoucher> Evouchers { get; set; }
        public DbSet<EVoucherToken> EvoucherTokens { get; set; }
        public DbSet<EVoucherRedeemLog> EvoucherRedeemLogs { get; set; }

        // 錢包歷史
        public DbSet<WalletHistory> WalletHistories { get; set; }

        // 管理員相關
        public DbSet<ManagerData> ManagerData { get; set; }
        public DbSet<ManagerRole> ManagerRoles { get; set; }
        public DbSet<ManagerRolePermission> ManagerRolePermissions { get; set; }

        // 論壇相關
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Thread> Threads { get; set; }
        public DbSet<ThreadPost> ThreadPosts { get; set; }
        public DbSet<Post> Posts { get; set; }

        // 社群相關
        public DbSet<Relation> Relations { get; set; }
        public DbSet<RelationStatus> RelationStatuses { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupChat> GroupChats { get; set; }
        public DbSet<GroupBlock> GroupBlocks { get; set; }

        // 通知相關
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }
        public DbSet<NotificationSource> NotificationSources { get; set; }
        public DbSet<NotificationAction> NotificationActions { get; set; }

        // 商城相關
        public DbSet<ProductInfo> ProductInfos { get; set; }
        public DbSet<OrderInfo> OrderInfos { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<OfficialStoreRanking> OfficialStoreRankings { get; set; }

        // 玩家市場相關
        public DbSet<PlayerMarketProductInfo> PlayerMarketProductInfos { get; set; }
        public DbSet<PlayerMarketProductImg> PlayerMarketProductImgs { get; set; }
        public DbSet<PlayerMarketOrderInfo> PlayerMarketOrderInfos { get; set; }
        public DbSet<PlayerMarketOrderTradepage> PlayerMarketOrderTradepages { get; set; }
        public DbSet<PlayerMarketTradeMsg> PlayerMarketTradeMsgs { get; set; }
        public DbSet<PlayerMarketRanking> PlayerMarketRankings { get; set; }

        // 遊戲相關
        public DbSet<Game> Games { get; set; }
        public DbSet<MetricSource> MetricSources { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<GameSourceMap> GameSourceMaps { get; set; }
        public DbSet<GameMetricDaily> GameMetricDailies { get; set; }
        public DbSet<LeaderboardSnapshot> LeaderboardSnapshots { get; set; }

        // 其他
        public DbSet<BannedWord> BannedWords { get; set; }
        public DbSet<Mute> Mutes { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 設定主鍵
            modelBuilder.Entity<User>().HasKey(u => u.UserId);
            modelBuilder.Entity<UserIntroduce>().HasKey(ui => ui.UserId);
            modelBuilder.Entity<UserRights>().HasKey(ur => ur.UserId);
            modelBuilder.Entity<UserWallet>().HasKey(uw => uw.UserId);
            modelBuilder.Entity<Pet>().HasKey(p => p.PetId);
            modelBuilder.Entity<MiniGame>().HasKey(mg => mg.PlayId);
            modelBuilder.Entity<CouponType>().HasKey(ct => ct.CouponTypeId);
            modelBuilder.Entity<Coupon>().HasKey(c => c.CouponId);
            modelBuilder.Entity<EVoucherType>().HasKey(evt => evt.EVoucherTypeId);
            modelBuilder.Entity<EVoucher>().HasKey(ev => ev.EVoucherId);
            modelBuilder.Entity<WalletHistory>().HasKey(wh => wh.HistoryId);

            // 設定外鍵關係
            modelBuilder.Entity<UserIntroduce>()
                .HasOne(ui => ui.User)
                .WithOne(u => u.UserIntroduce)
                .HasForeignKey<UserIntroduce>(ui => ui.UserId);

            modelBuilder.Entity<UserRights>()
                .HasOne(ur => ur.User)
                .WithOne(u => u.UserRights)
                .HasForeignKey<UserRights>(ur => ur.UserId);

            modelBuilder.Entity<UserWallet>()
                .HasOne(uw => uw.User)
                .WithOne(u => u.UserWallet)
                .HasForeignKey<UserWallet>(uw => uw.UserId);

            modelBuilder.Entity<Pet>()
                .HasOne(p => p.User)
                .WithMany(u => u.Pets)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<MiniGame>()
                .HasOne(mg => mg.User)
                .WithMany(u => u.MiniGames)
                .HasForeignKey(mg => mg.UserId);

            modelBuilder.Entity<MiniGame>()
                .HasOne(mg => mg.Pet)
                .WithMany(p => p.MiniGames)
                .HasForeignKey(mg => mg.PetId);

            modelBuilder.Entity<Coupon>()
                .HasOne(c => c.User)
                .WithMany(u => u.Coupons)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Coupon>()
                .HasOne(c => c.CouponType)
                .WithMany(ct => ct.Coupons)
                .HasForeignKey(c => c.CouponTypeId);

            modelBuilder.Entity<EVoucher>()
                .HasOne(ev => ev.User)
                .WithMany(u => u.EVouchers)
                .HasForeignKey(ev => ev.UserId);

            modelBuilder.Entity<EVoucher>()
                .HasOne(ev => ev.EVoucherType)
                .WithMany(evt => evt.EVouchers)
                .HasForeignKey(ev => ev.EVoucherTypeId);

            modelBuilder.Entity<WalletHistory>()
                .HasOne(wh => wh.User)
                .WithMany(u => u.WalletHistories)
                .HasForeignKey(wh => wh.UserId);
        }
    }
}