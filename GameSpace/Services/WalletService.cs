using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Services
{
    public interface IWalletService
    {
        Task<UserWallet?> GetUserWalletAsync(int userId);
        Task<UserWallet> CreateUserWalletAsync(int userId);
        Task<bool> AddPointsAsync(int userId, int points, string reason);
        Task<bool> DeductPointsAsync(int userId, int points, string reason);
        Task<bool> TransferPointsAsync(int fromUserId, int toUserId, int points, string reason);
        Task<List<WalletTransaction>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 20);
        Task<bool> CanAffordAsync(int userId, int points);
        Task<int> GetUserBalanceAsync(int userId);
    }

    public class WalletService : IWalletService
    {
        private readonly GameSpaceDbContext _context;

        public WalletService(GameSpaceDbContext context)
        {
            _context = context;
        }

        public async Task<UserWallet?> GetUserWalletAsync(int userId)
        {
            return await _context.UserWallets
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<UserWallet> CreateUserWalletAsync(int userId)
        {
            var wallet = new UserWallet
            {
                UserId = userId,
                UserPoint = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserWallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task<bool> AddPointsAsync(int userId, int points, string reason)
        {
            if (points <= 0) return false;

            var wallet = await GetUserWalletAsync(userId);
            if (wallet == null)
            {
                wallet = await CreateUserWalletAsync(userId);
            }

            wallet.UserPoint += points;
            wallet.UpdatedAt = DateTime.UtcNow;

            // 記錄交易
            var transaction = new WalletTransaction
            {
                UserId = userId,
                Amount = points,
                TransactionType = "Credit",
                Description = reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeductPointsAsync(int userId, int points, string reason)
        {
            if (points <= 0) return false;

            var wallet = await GetUserWalletAsync(userId);
            if (wallet == null || wallet.UserPoint < points) return false;

            wallet.UserPoint -= points;
            wallet.UpdatedAt = DateTime.UtcNow;

            // 記錄交易
            var transaction = new WalletTransaction
            {
                UserId = userId,
                Amount = -points,
                TransactionType = "Debit",
                Description = reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TransferPointsAsync(int fromUserId, int toUserId, int points, string reason)
        {
            if (points <= 0) return false;

            var fromWallet = await GetUserWalletAsync(fromUserId);
            var toWallet = await GetUserWalletAsync(toUserId);

            if (fromWallet == null || toWallet == null) return false;
            if (fromWallet.UserPoint < points) return false;

            // 扣除發送者點數
            fromWallet.UserPoint -= points;
            fromWallet.UpdatedAt = DateTime.UtcNow;

            // 增加接收者點數
            toWallet.UserPoint += points;
            toWallet.UpdatedAt = DateTime.UtcNow;

            // 記錄交易
            var fromTransaction = new WalletTransaction
            {
                UserId = fromUserId,
                Amount = -points,
                TransactionType = "Transfer_Out",
                Description = $"轉帳給用戶 {toUserId}: {reason}",
                CreatedAt = DateTime.UtcNow
            };

            var toTransaction = new WalletTransaction
            {
                UserId = toUserId,
                Amount = points,
                TransactionType = "Transfer_In",
                Description = $"來自用戶 {fromUserId} 的轉帳: {reason}",
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.AddRange(fromTransaction, toTransaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<WalletTransaction>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.WalletTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> CanAffordAsync(int userId, int points)
        {
            var wallet = await GetUserWalletAsync(userId);
            return wallet != null && wallet.UserPoint >= points;
        }

        public async Task<int> GetUserBalanceAsync(int userId)
        {
            var wallet = await GetUserWalletAsync(userId);
            return wallet?.UserPoint ?? 0;
        }
    }
}