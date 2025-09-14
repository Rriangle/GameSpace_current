using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class MiniGameController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public MiniGameController(GameSpaceDbContext context)
        {
            _context = context;
        }

        // 小遊戲首頁
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);
            var todayGames = await _context.MiniGames
                .Where(mg => mg.UserId == userId && mg.StartTime.Date == DateTime.UtcNow.Date)
                .CountAsync();

            var canPlay = pet != null && pet.Health > 0 && todayGames < 3;

            ViewBag.CanPlay = canPlay;
            ViewBag.TodayGames = todayGames;
            ViewBag.RemainingGames = Math.Max(0, 3 - todayGames);

            return View();
        }

        // 開始遊戲
        [HttpPost]
        public async Task<IActionResult> StartGame()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "找不到寵物" });
            }

            if (pet.Health <= 0)
            {
                return Json(new { success = false, message = "寵物健康值不足，無法開始遊戲" });
            }

            var todayGames = await _context.MiniGames
                .Where(mg => mg.UserId == userId && mg.StartTime.Date == DateTime.UtcNow.Date)
                .CountAsync();

            if (todayGames >= 3)
            {
                return Json(new { success = false, message = "今日遊戲次數已達上限" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 創建遊戲記錄
                var miniGame = new MiniGame
                {
                    UserId = userId,
                    PetId = pet.PetId,
                    Level = GetNextLevel(userId),
                    MonsterCount = GetMonsterCount(GetNextLevel(userId)),
                    SpeedMultiplier = GetSpeedMultiplier(GetNextLevel(userId)),
                    StartTime = DateTime.UtcNow,
                    Result = "Unknown"
                };

                _context.MiniGames.Add(miniGame);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "遊戲開始！",
                    gameId = miniGame.PlayId,
                    level = miniGame.Level,
                    monsterCount = miniGame.MonsterCount,
                    speedMultiplier = miniGame.SpeedMultiplier
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "遊戲開始失敗" });
            }
        }

        // 結束遊戲
        [HttpPost]
        public async Task<IActionResult> EndGame(int gameId, string result, int monstersDefeated, int timeSpent)
        {
            var userId = GetCurrentUserId();
            var miniGame = await _context.MiniGames
                .FirstOrDefaultAsync(mg => mg.PlayId == gameId && mg.UserId == userId);

            if (miniGame == null)
            {
                return Json(new { success = false, message = "找不到遊戲記錄" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 更新遊戲結果
                miniGame.Result = result;
                miniGame.EndTime = DateTime.UtcNow;
                miniGame.Aborted = result == "Abort";

                // 計算獎勵
                var rewards = CalculateRewards(miniGame, result, monstersDefeated, timeSpent);
                miniGame.ExpGained = rewards.ExpGained;
                miniGame.PointsChanged = rewards.PointsChanged;
                miniGame.CouponGained = rewards.CouponGained;
                miniGame.HungerDelta = rewards.HungerDelta;
                miniGame.MoodDelta = rewards.MoodDelta;
                miniGame.StaminaDelta = rewards.StaminaDelta;
                miniGame.CleanlinessDelta = rewards.CleanlinessDelta;

                // 更新寵物狀態
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == miniGame.PetId);
                if (pet != null)
                {
                    pet.Hunger = Math.Max(0, Math.Min(100, pet.Hunger + rewards.HungerDelta));
                    pet.Mood = Math.Max(0, Math.Min(100, pet.Mood + rewards.MoodDelta));
                    pet.Stamina = Math.Max(0, Math.Min(100, pet.Stamina + rewards.StaminaDelta));
                    pet.Cleanliness = Math.Max(0, Math.Min(100, pet.Cleanliness + rewards.CleanlinessDelta));

                    if (rewards.ExpGained > 0)
                    {
                        pet.Experience += rewards.ExpGained;
                        miniGame.ExpGainedTime = DateTime.UtcNow;

                        // 檢查升級
                        var requiredExp = CalculateRequiredExp(pet.Level);
                        if (pet.Experience >= requiredExp)
                        {
                            pet.Level++;
                            pet.Experience -= requiredExp;
                            pet.LevelUpTime = DateTime.UtcNow;

                            var levelUpReward = CalculateLevelUpReward(pet.Level);
                            pet.PointsGainedLevelUp = levelUpReward;
                            pet.PointsGainedTimeLevelUp = DateTime.UtcNow;
                            miniGame.PointsChanged += levelUpReward;
                        }
                    }
                }

                // 更新用戶點數
                if (rewards.PointsChanged > 0)
                {
                    var userWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);
                    if (userWallet != null)
                    {
                        userWallet.UserPoint += rewards.PointsChanged;
                        miniGame.PointsChangedTime = DateTime.UtcNow;

                        // 記錄錢包歷史
                        _context.WalletHistories.Add(new WalletHistory
                        {
                            UserId = userId,
                            ChangeType = "Point",
                            PointsChanged = rewards.PointsChanged,
                            Description = "小遊戲獎勵",
                            ChangeTime = DateTime.UtcNow
                        });
                    }
                }

                // 發放優惠券
                if (rewards.CouponGained > 0)
                {
                    var couponType = await _context.CouponTypes.FirstOrDefaultAsync();
                    if (couponType != null)
                    {
                        var coupon = new Coupon
                        {
                            UserId = userId,
                            CouponTypeId = couponType.CouponTypeId,
                            CouponCode = GenerateCouponCode(),
                            IsUsed = false,
                            AcquiredTime = DateTime.UtcNow
                        };
                        _context.Coupons.Add(coupon);
                        miniGame.CouponGainedTime = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = GetResultMessage(result, rewards),
                    expGained = rewards.ExpGained,
                    pointsChanged = rewards.PointsChanged,
                    couponGained = rewards.CouponGained,
                    level = pet?.Level ?? 0,
                    experience = pet?.Experience ?? 0
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "遊戲結束失敗" });
            }
        }

        // 遊戲歷史
        public async Task<IActionResult> History()
        {
            var userId = GetCurrentUserId();
            var games = await _context.MiniGames
                .Where(mg => mg.UserId == userId)
                .OrderByDescending(mg => mg.StartTime)
                .Take(20)
                .ToListAsync();

            return View(games);
        }

        private int GetNextLevel(int userId)
        {
            var lastGame = _context.MiniGames
                .Where(mg => mg.UserId == userId && mg.Result == "Win")
                .OrderByDescending(mg => mg.StartTime)
                .FirstOrDefault();

            if (lastGame == null)
                return 1; // 第一關

            return Math.Min(lastGame.Level + 1, 3); // 最多第三關
        }

        private int GetMonsterCount(int level)
        {
            return level switch
            {
                1 => 6,
                2 => 8,
                3 => 10,
                _ => 6
            };
        }

        private decimal GetSpeedMultiplier(int level)
        {
            return level switch
            {
                1 => 1.00m,
                2 => 1.50m,
                3 => 2.00m,
                _ => 1.00m
            };
        }

        private (int ExpGained, int PointsChanged, int CouponGained, int HungerDelta, int MoodDelta, int StaminaDelta, int CleanlinessDelta) CalculateRewards(MiniGame miniGame, string result, int monstersDefeated, int timeSpent)
        {
            if (result == "Abort")
            {
                return (0, 0, 0, -10, -10, -10, -5);
            }

            if (result == "Lose")
            {
                return (5, 0, 0, -20, -30, -20, -20);
            }

            // Win
            var baseExp = miniGame.Level * 50;
            var basePoints = miniGame.Level * 10;
            var expGained = baseExp + (monstersDefeated * 10);
            var pointsChanged = basePoints + (monstersDefeated * 5);
            var couponGained = miniGame.Level == 3 ? 1 : 0;

            return (expGained, pointsChanged, couponGained, -20, 30, -20, -20);
        }

        private string GetResultMessage(string result, (int ExpGained, int PointsChanged, int CouponGained, int HungerDelta, int MoodDelta, int StaminaDelta, int CleanlinessDelta) rewards)
        {
            return result switch
            {
                "Win" => $"勝利！獲得 {rewards.ExpGained} 經驗值、{rewards.PointsChanged} 點數" + (rewards.CouponGained > 0 ? "和優惠券" : ""),
                "Lose" => "失敗了，但獲得了少量經驗值",
                "Abort" => "遊戲中途退出",
                _ => "遊戲結束"
            };
        }

        private int CalculateRequiredExp(int level)
        {
            if (level <= 10)
                return 40 * level + 60;
            else if (level <= 100)
                return (int)(0.8 * level * level + 380);
            else
                return (int)(285.69 * Math.Pow(1.06, level));
        }

        private int CalculateLevelUpReward(int level)
        {
            if (level <= 10)
                return level * 10;
            else if (level <= 20)
                return level * 20;
            else if (level <= 30)
                return level * 30;
            else if (level <= 40)
                return level * 40;
            else if (level <= 50)
                return level * 50;
            else
                return Math.Min(level * 60, 3000);
        }

        private string GenerateCouponCode()
        {
            return $"COUPON{Random.Shared.Next(100000, 999999)}";
        }

        private int GetCurrentUserId()
        {
            // 暫時返回固定用戶ID，實際應該從認證中獲取
            return 1;
        }
    }
}