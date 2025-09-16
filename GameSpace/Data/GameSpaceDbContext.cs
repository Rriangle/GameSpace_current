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
        public DbSet<UserRight> UserRights { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<UserSignInStats> UserSignInStats { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
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
        public DbSet<PlayerMarketProductImage> PlayerMarketProductImgs { get; set; }
        public DbSet<PlayerMarketOrderInfo> PlayerMarketOrderInfos { get; set; }
        public DbSet<PlayerMarketOrderTradePage> PlayerMarketOrderTradepages { get; set; }
        public DbSet<PlayerMarketTradeMessage> PlayerMarketTradeMsgs { get; set; }
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
            modelBuilder.Entity<UserRight>().HasKey(ur => ur.UserId);
            modelBuilder.Entity<UserWallet>().HasKey(uw => uw.UserID);
            modelBuilder.Entity<Pet>().HasKey(p => p.PetID);
            modelBuilder.Entity<MiniGame>().HasKey(mg => mg.PlayID);
            modelBuilder.Entity<CouponType>().HasKey(ct => ct.CouponTypeId);
            modelBuilder.Entity<Coupon>().HasKey(c => c.CouponId);
            modelBuilder.Entity<EVoucherType>().HasKey(evt => evt.EVoucherTypeId);
            modelBuilder.Entity<EVoucher>().HasKey(ev => ev.EVoucherId);
            modelBuilder.Entity<WalletHistory>().HasKey(wh => wh.HistoryId);

            // 效能優化：資料庫索引配置
            ConfigureIndexes(modelBuilder);

            // 設定外鍵關係
            modelBuilder.Entity<UserIntroduce>()
                .HasOne(ui => ui.User)
                .WithOne(u => u.UserIntroduce)
                .HasForeignKey<UserIntroduce>(ui => ui.UserId);

            modelBuilder.Entity<UserRight>()
                .HasOne(ur => ur.User)
                .WithOne(u => u.UserRight)
                .HasForeignKey<UserRight>(ur => ur.UserId);

            modelBuilder.Entity<UserWallet>()
                .HasOne(uw => uw.User)
                .WithOne(u => u.UserWallet)
                .HasForeignKey<UserWallet>(uw => uw.UserID);

            modelBuilder.Entity<Pet>()
                .HasOne(p => p.User)
                .WithMany(u => u.Pets)
                .HasForeignKey(p => p.UserID);

            modelBuilder.Entity<MiniGame>()
                .HasOne(mg => mg.User)
                .WithMany(u => u.MiniGames)
                .HasForeignKey(mg => mg.UserID);

            modelBuilder.Entity<MiniGame>()
                .HasOne(mg => mg.Pet)
                .WithMany(p => p.MiniGames)
                .HasForeignKey(mg => mg.PetID);

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

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // 用戶相關索引
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserAccount)
                .IsUnique()
                .HasDatabaseName("IX_Users_UserAccount");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserEmail)
                .IsUnique()
                .HasDatabaseName("IX_Users_UserEmail");

            modelBuilder.Entity<UserIntroduce>()
                .HasIndex(ui => ui.UserNickName)
                .IsUnique()
                .HasDatabaseName("IX_UserIntroduces_UserNickName");

            modelBuilder.Entity<UserIntroduce>()
                .HasIndex(ui => ui.Email)
                .IsUnique()
                .HasDatabaseName("IX_UserIntroduces_Email");

            modelBuilder.Entity<UserIntroduce>()
                .HasIndex(ui => ui.Cellphone)
                .IsUnique()
                .HasDatabaseName("IX_UserIntroduces_Cellphone");

            // 寵物相關索引
            modelBuilder.Entity<Pet>()
                .HasIndex(p => p.UserId)
                .HasDatabaseName("IX_Pets_UserId");

            modelBuilder.Entity<Pet>()
                .HasIndex(p => new { p.UserId, p.Level })
                .HasDatabaseName("IX_Pets_UserId_Level");

            // 小遊戲相關索引
            modelBuilder.Entity<MiniGame>()
                .HasIndex(mg => mg.UserId)
                .HasDatabaseName("IX_MiniGames_UserId");

            modelBuilder.Entity<MiniGame>()
                .HasIndex(mg => mg.PetId)
                .HasDatabaseName("IX_MiniGames_PetId");

            modelBuilder.Entity<MiniGame>()
                .HasIndex(mg => new { mg.UserId, mg.StartTime })
                .HasDatabaseName("IX_MiniGames_UserId_StartTime");

            modelBuilder.Entity<MiniGame>()
                .HasIndex(mg => mg.Result)
                .HasDatabaseName("IX_MiniGames_Result");

            // 簽到相關索引
            modelBuilder.Entity<UserSignInStats>()
                .HasIndex(us => us.UserId)
                .HasDatabaseName("IX_UserSignInStats_UserId");

            modelBuilder.Entity<UserSignInStats>()
                .HasIndex(us => new { us.UserId, us.SignTime })
                .HasDatabaseName("IX_UserSignInStats_UserId_SignTime");

            modelBuilder.Entity<UserSignInStats>()
                .HasIndex(us => us.SignTime)
                .HasDatabaseName("IX_UserSignInStats_SignTime");

            // 錢包相關索引
            modelBuilder.Entity<WalletHistory>()
                .HasIndex(wh => wh.UserId)
                .HasDatabaseName("IX_WalletHistories_UserId");

            modelBuilder.Entity<WalletHistory>()
                .HasIndex(wh => new { wh.UserId, wh.ChangeTime })
                .HasDatabaseName("IX_WalletHistories_UserId_ChangeTime");

            modelBuilder.Entity<WalletHistory>()
                .HasIndex(wh => wh.ChangeType)
                .HasDatabaseName("IX_WalletHistories_ChangeType");

            // 優惠券相關索引
            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Coupons_UserId");

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.CouponCode)
                .IsUnique()
                .HasDatabaseName("IX_Coupons_CouponCode");

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => new { c.UserId, c.IsUsed })
                .HasDatabaseName("IX_Coupons_UserId_IsUsed");

            // 論壇相關索引
            modelBuilder.Entity<Thread>()
                .HasIndex(t => t.ForumId)
                .HasDatabaseName("IX_Threads_ForumId");

            modelBuilder.Entity<Thread>()
                .HasIndex(t => new { t.ForumId, t.UpdatedAt })
                .HasDatabaseName("IX_Threads_ForumId_UpdatedAt");

            modelBuilder.Entity<ThreadPost>()
                .HasIndex(tp => tp.ThreadId)
                .HasDatabaseName("IX_ThreadPosts_ThreadId");

            modelBuilder.Entity<ThreadPost>()
                .HasIndex(tp => new { tp.ThreadId, tp.CreatedAt })
                .HasDatabaseName("IX_ThreadPosts_ThreadId_CreatedAt");

            // 聊天相關索引
            modelBuilder.Entity<ChatMessage>()
                .HasIndex(cm => new { cm.SenderId, cm.ReceiverId })
                .HasDatabaseName("IX_ChatMessages_SenderId_ReceiverId");

            modelBuilder.Entity<ChatMessage>()
                .HasIndex(cm => cm.SentAt)
                .HasDatabaseName("IX_ChatMessages_SentAt");

            // 商城相關索引
            modelBuilder.Entity<OrderInfo>()
                .HasIndex(oi => oi.UserId)
                .HasDatabaseName("IX_OrderInfos_UserId");

            modelBuilder.Entity<OrderInfo>()
                .HasIndex(oi => new { oi.UserId, oi.OrderDate })
                .HasDatabaseName("IX_OrderInfos_UserId_OrderDate");

            modelBuilder.Entity<OrderInfo>()
                .HasIndex(oi => oi.OrderStatus)
                .HasDatabaseName("IX_OrderInfos_OrderStatus");

            // 遊戲熱度相關索引
            modelBuilder.Entity<GameMetricDaily>()
                .HasIndex(gmd => new { gmd.GameId, gmd.MetricId, gmd.Date })
                .IsUnique()
                .HasDatabaseName("IX_GameMetricDailies_GameId_MetricId_Date");

            modelBuilder.Entity<LeaderboardSnapshot>()
                .HasIndex(ls => new { ls.Period, ls.Ts, ls.Rank })
                .HasDatabaseName("IX_LeaderboardSnapshots_Period_Ts_Rank");

            // 通知相關索引
            modelBuilder.Entity<NotificationRecipient>()
                .HasIndex(nr => new { nr.UserId, nr.IsRead })
                .HasDatabaseName("IX_NotificationRecipients_UserId_IsRead");

            modelBuilder.Entity<NotificationRecipient>()
                .HasIndex(nr => nr.UserId)
                .HasDatabaseName("IX_NotificationRecipients_UserId");
        }
    }
}