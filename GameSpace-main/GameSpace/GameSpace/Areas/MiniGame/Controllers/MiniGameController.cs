using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class MiniGameController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public MiniGameController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // GET: MiniGame/MiniGame - 取得小遊戲記錄列表
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var games = await _context.MiniGames
                .Where(g => g.UserId == userId && !g.Aborted)
                .OrderByDescending(g => g.StartTime)
                .Take(20) // 只顯示最近20筆記錄
                .ToListAsync();

            // 檢查今日已玩次數
            var today = DateTime.UtcNow.Date;
            var todayGamesCount = await _context.MiniGames
                .CountAsync(g => g.UserId == userId && g.StartTime.Date == today && !g.Aborted);
            
            ViewBag.TodayGamesCount = todayGamesCount;
            ViewBag.MaxDailyGames = 3;

            return View(games);
        }

        // GET: MiniGame/MiniGame/StartGame - 顯示開始遊戲頁面
        public async Task<IActionResult> StartGame()
        {
            var userId = GetCurrentUserId();
            var today = DateTime.UtcNow.Date;

            // 檢查每日遊戲限制（每天3場遊戲）
            var todayGames = await _context.MiniGames
                .CountAsync(g => g.UserId == userId && g.StartTime.Date == today && !g.Aborted);

            if (todayGames >= 3)
            {
                TempData["Warning"] = "已達到每日遊戲限制（3次）！";
                return RedirectToAction(nameof(Index));
            }

            // 取得使用者的寵物
            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                TempData["Error"] = "找不到寵物！請先創建寵物。";
                return RedirectToAction("Index", "Pet");
            }

            // 檢查寵物狀態
            if (pet.Health <= 0 || pet.Hunger >= 100 || pet.Mood <= 0 || pet.Stamina <= 0 || pet.Cleanliness <= 0)
            {
                TempData["Warning"] = "寵物狀態不佳，無法開始遊戲！請先照顧寵物。";
                return RedirectToAction("Index", "Pet");
            }

            ViewBag.Pet = pet;
            ViewBag.TodayGamesCount = todayGames;
            return View();
        }

        // POST: MiniGame/MiniGame/Start - 開始新遊戲
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int level = 1)
        {
            var userId = GetCurrentUserId();
            var today = DateTime.UtcNow.Date;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 檢查每日遊戲限制
                var todayGames = await _context.MiniGames
                    .CountAsync(g => g.UserId == userId && g.StartTime.Date == today && !g.Aborted);

                if (todayGames >= 3)
                {
                    return Json(new { success = false, message = "已達到每日遊戲限制（3次）" });
                }

                // 取得寵物
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (pet == null)
                {
                    return Json(new { success = false, message = "找不到寵物" });
                }

                // 檢查寵物狀態
                if (pet.Health <= 0 || pet.Hunger >= 100 || pet.Mood <= 0 || pet.Stamina <= 0 || pet.Cleanliness <= 0)
                {
                    return Json(new { success = false, message = "寵物狀態不佳，無法開始遊戲" });
                }

                // 根據關卡設定遊戲參數
                var (monsterCount, speedMultiplier) = GetLevelConfig(level);

                // 建立新遊戲記錄
                var game = new Models.MiniGame
                {
                    UserId = userId,
                    PetId = pet.PetId,
                    Level = level,
                    MonsterCount = monsterCount,
                    SpeedMultiplier = speedMultiplier,
                    Result = "Unknown",
                    ExpGained = 0,
                    ExpGainedTime = DateTime.UtcNow,
                    PointsGained = 0,
                    PointsGainedTime = DateTime.UtcNow,
                    CouponGained = "0",
                    CouponGainedTime = DateTime.UtcNow,
                    HungerDelta = 0,
                    MoodDelta = 0,
                    StaminaDelta = 0,
                    CleanlinessDelta = 0,
                    StartTime = DateTime.UtcNow,
                    EndTime = null,
                    Aborted = false
                };

                _context.MiniGames.Add(game);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "遊戲已開始！", 
                    gameId = game.PlayId,
                    level = level,
                    monsterCount = monsterCount,
                    speedMultiplier = speedMultiplier
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"開始遊戲失敗：{ex.Message}" });
            }
        }

        // GET: MiniGame/MiniGame/Play - 遊戲畫面
        public async Task<IActionResult> Play(int gameId)
        {
            var game = await _context.MiniGames
                .Include(g => g.User)
                .FirstOrDefaultAsync(g => g.PlayId == gameId);

            if (game == null || game.UserId != GetCurrentUserId())
            {
                TempData["Error"] = "找不到遊戲記錄！";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(game.Result) && game.Result != "Unknown")
            {
                TempData["Warning"] = "遊戲已結束！";
                return RedirectToAction(nameof(Index));
            }

            return View(game);
        }

        // POST: MiniGame/MiniGame/End - 結束遊戲
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> End(int gameId, string result, bool aborted = false)
        {
            var userId = GetCurrentUserId();
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var game = await _context.MiniGames
                    .FirstOrDefaultAsync(g => g.PlayId == gameId && g.UserId == userId);

                if (game == null)
                {
                    return Json(new { success = false, message = "找不到遊戲記錄" });
                }

                if (game.Result != "Unknown")
                {
                    return Json(new { success = false, message = "遊戲已結束" });
                }

                // 更新遊戲記錄
                game.Result = aborted ? "Abort" : result;
                game.EndTime = DateTime.UtcNow;
                game.Aborted = aborted;

                // 如果是放棄，不給任何獎勵
                if (aborted)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "遊戲已中止", result = "Abort" });
                }

                // 計算獎勵和寵物狀態變化
                var (expGained, pointsGained, couponCode, hungerDelta, moodDelta, staminaDelta, cleanlinessDelta) = 
                    CalculateGameRewards(game.Level, result);

                game.ExpGained = expGained;
                game.PointsGained = pointsGained;
                game.CouponGained = couponCode;
                game.HungerDelta = hungerDelta;
                game.MoodDelta = moodDelta;
                game.StaminaDelta = staminaDelta;
                game.CleanlinessDelta = cleanlinessDelta;

                // 更新寵物狀態
                var pet = await _context.Pets
                    .FirstOrDefaultAsync(p => p.PetId == game.PetId);

                if (pet != null)
                {
                    pet.Hunger = Math.Max(0, Math.Min(100, pet.Hunger + hungerDelta));
                    pet.Mood = Math.Max(0, Math.Min(100, pet.Mood + moodDelta));
                    pet.Stamina = Math.Max(0, Math.Min(100, pet.Stamina + staminaDelta));
                    pet.Cleanliness = Math.Max(0, Math.Min(100, pet.Cleanliness + cleanlinessDelta));
                    pet.Experience += expGained;

                    // 檢查升級
                    await CheckPetLevelUp(pet, userId);
                }

                // 更新錢包點數
                if (pointsGained > 0)
                {
                    var wallet = await _context.UserWallets
                        .FirstOrDefaultAsync(w => w.UserId == userId);
                    if (wallet == null)
                    {
                        wallet = new UserWallet { UserId = userId, UserPoint = 0 };
                        _context.UserWallets.Add(wallet);
                    }
                    wallet.UserPoint += pointsGained;

                    // 記錄錢包異動
                    var history = new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "MiniGame",
                        PointsChanged = pointsGained,
                        Description = $"小遊戲{result}獲得（Lv.{game.Level}）",
                        ChangeTime = DateTime.UtcNow
                    };
                    _context.WalletHistories.Add(history);
                }

                // 如果有優惠券獎勵
                if (!string.IsNullOrEmpty(couponCode) && couponCode != "0")
                {
                    await CreateGameCoupon(userId, couponCode);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var message = $"遊戲{(result == "Win" ? "勝利" : "失敗")}！";
                if (pointsGained > 0) message += $" 獲得{pointsGained}點數";
                if (expGained > 0) message += $"、{expGained}寵物經驗";
                if (!string.IsNullOrEmpty(couponCode) && couponCode != "0") message += $"、優惠券";

                return Json(new { 
                    success = true, 
                    message = message,
                    result = result,
                    expGained = expGained,
                    pointsGained = pointsGained,
                    couponGained = couponCode != "0" ? couponCode : null
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"結束遊戲失敗：{ex.Message}" });
            }
        }

        // 取得關卡配置
        private (int monsterCount, decimal speedMultiplier) GetLevelConfig(int level)
        {
            return level switch
            {
                1 => (6, 1.0m),
                2 => (8, 1.5m),
                3 => (10, 2.0m),
                _ => (6, 1.0m) // 預設第1關
            };
        }

        // 計算遊戲獎勵
        private (int expGained, int pointsGained, string couponCode, int hungerDelta, int moodDelta, int staminaDelta, int cleanlinessDelta) 
            CalculateGameRewards(int level, string result)
        {
            int expGained = 0, pointsGained = 0;
            string couponCode = "0";
            int hungerDelta, moodDelta, staminaDelta, cleanlinessDelta;

            if (result == "Win")
            {
                // 勝利獎勵
                expGained = level switch
                {
                    1 => 100,
                    2 => 200,
                    3 => 300,
                    _ => 100
                };
                
                pointsGained = level switch
                {
                    1 => 10,
                    2 => 20,
                    3 => 30,
                    _ => 10
                };

                // 第3關勝利額外給優惠券
                if (level == 3)
                {
                    couponCode = "GAME_WIN_L3";
                }

                // 勝利狀態變化
                hungerDelta = -20;  // 消耗體力會餓
                moodDelta = +30;    // 勝利很開心
                staminaDelta = -20; // 消耗體力
                cleanlinessDelta = -20; // 弄髒了
            }
            else
            {
                // 失敗獎勵（較少）
                expGained = 20;
                pointsGained = 5;

                // 失敗狀態變化
                hungerDelta = -20;  // 同樣會餓
                moodDelta = -30;    // 失敗心情不好
                staminaDelta = -20; // 消耗體力
                cleanlinessDelta = -20; // 弄髒了
            }

            return (expGained, pointsGained, couponCode, hungerDelta, moodDelta, staminaDelta, cleanlinessDelta);
        }

        // 建立遊戲獎勵優惠券
        private async Task CreateGameCoupon(int userId, string couponType)
        {
            // 找到遊戲獎勵相關的優惠券類型
            var couponTypeRecord = await _context.CouponTypes
                .FirstOrDefaultAsync(ct => ct.Name.Contains("遊戲獎勵") || ct.Name.Contains("冒險獎勵"));

            if (couponTypeRecord != null)
            {
                var couponCode = GenerateCouponCode();
                var coupon = new Coupon
                {
                    CouponCode = couponCode,
                    CouponTypeId = couponTypeRecord.CouponTypeId,
                    UserId = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.UtcNow
                };
                _context.Coupons.Add(coupon);

                // 記錄錢包異動
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Coupon",
                    PointsChanged = 0,
                    ItemCode = couponCode,
                    Description = $"小遊戲獎勵優惠券：{couponTypeRecord.Name}",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);
            }
        }

        // 檢查寵物升級
        private async Task CheckPetLevelUp(Pet pet, int userId)
        {
            int requiredExp = CalculateRequiredExp(pet.Level);
            
            if (pet.Experience >= requiredExp)
            {
                pet.Level++;
                pet.Experience -= requiredExp;
                pet.LevelUpTime = DateTime.UtcNow;
                
                int pointsReward = Math.Min(250, (pet.Level / 10 + 1) * 10);
                pet.PointsGainedLevelUp = pointsReward;
                pet.PointsGainedTimeLevelUp = DateTime.UtcNow;
                
                var wallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);
                if (wallet != null)
                {
                    wallet.UserPoint += pointsReward;
                }

                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "PetLevelUp",
                    PointsChanged = pointsReward,
                    Description = $"寵物升級獎勵（Lv.{pet.Level}）",
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);
            }
        }

        private int CalculateRequiredExp(int level)
        {
            if (level <= 10)
            {
                return 40 * level + 60;
            }
            else if (level <= 100)
            {
                return (int)(0.8 * level * level + 380);
            }
            else
            {
                return (int)(285.69 * Math.Pow(1.06, level));
            }
        }

        private string GenerateCouponCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private int GetCurrentUserId()
        {
            // 暫時使用固定用戶ID進行測試
            // TODO: 實作適當的用戶身份驗證
            return 1;
        }
    }
}