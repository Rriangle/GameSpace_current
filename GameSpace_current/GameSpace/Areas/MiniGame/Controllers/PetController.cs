using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class PetController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public PetController(GameSpaceDbContext context)
        {
            _context = context;
        }

        // 取得寵物狀態
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                // 如果沒有寵物，創建一個
                pet = await CreatePetForUser(userId);
            }

            return View(pet);
        }

        // 餵食寵物
        [HttpPost]
        public async Task<IActionResult> Feed()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "找不到寵物" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 餵食：飢餓值 +10，心情值 +10
                pet.Hunger = Math.Min(100, pet.Hunger + 10);
                pet.Mood = Math.Min(100, pet.Mood + 10);
                pet.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "餵食成功！寵物很開心",
                    hunger = pet.Hunger,
                    mood = pet.Mood
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "餵食失敗" });
            }
        }

        // 洗澡寵物
        [HttpPost]
        public async Task<IActionResult> Bath()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "找不到寵物" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 洗澡：清潔值 +10
                pet.Cleanliness = Math.Min(100, pet.Cleanliness + 10);
                pet.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "洗澡完成！寵物變乾淨了",
                    cleanliness = pet.Cleanliness
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "洗澡失敗" });
            }
        }

        // 陪玩寵物
        [HttpPost]
        public async Task<IActionResult> Play()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "找不到寵物" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 陪玩：心情值 +10，體力值 -5
                pet.Mood = Math.Min(100, pet.Mood + 10);
                pet.Stamina = Math.Max(0, pet.Stamina - 5);
                pet.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "陪玩完成！寵物很開心",
                    mood = pet.Mood,
                    stamina = pet.Stamina
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "陪玩失敗" });
            }
        }

        // 哄睡寵物
        [HttpPost]
        public async Task<IActionResult> Sleep()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "找不到寵物" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 哄睡：體力值 +10，心情值 +5
                pet.Stamina = Math.Min(100, pet.Stamina + 10);
                pet.Mood = Math.Min(100, pet.Mood + 5);
                pet.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "哄睡完成！寵物休息得很好",
                    stamina = pet.Stamina,
                    mood = pet.Mood
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "哄睡失敗" });
            }
        }

        // 換膚色
        [HttpPost]
        public async Task<IActionResult> ChangeSkinColor(string color)
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);
            var userWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);

            if (pet == null || userWallet == null)
            {
                return Json(new { success = false, message = "找不到寵物或錢包" });
            }

            const int skinColorCost = 2000;
            if (userWallet.UserPoint < skinColorCost)
            {
                return Json(new { success = false, message = "點數不足，需要 2000 點" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 扣除點數
                userWallet.UserPoint -= skinColorCost;
                pet.SkinColor = color;
                pet.ColorChangedTime = DateTime.UtcNow;
                pet.PointsChangedColor = skinColorCost;
                pet.PointsChangedTimeColor = DateTime.UtcNow;

                // 記錄錢包歷史
                _context.WalletHistories.Add(new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = -skinColorCost,
                    Description = "購買寵物膚色",
                    ChangeTime = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "膚色更換成功！",
                    skinColor = pet.SkinColor,
                    points = userWallet.UserPoint
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "膚色更換失敗" });
            }
        }

        // 換背景色
        [HttpPost]
        public async Task<IActionResult> ChangeBackgroundColor(string color)
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);

            if (pet == null)
            {
                return Json(new { success = false, message = "找不到寵物" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 換背景色免費
                pet.BackgroundColor = color;
                pet.BackgroundColorChangedTime = DateTime.UtcNow;
                pet.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { 
                    success = true, 
                    message = "背景色更換成功！",
                    backgroundColor = pet.BackgroundColor
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "背景色更換失敗" });
            }
        }

        // 檢查寵物升級
        [HttpPost]
        public async Task<IActionResult> CheckLevelUp()
        {
            var userId = GetCurrentUserId();
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == userId);
            var userWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == userId);

            if (pet == null || userWallet == null)
            {
                return Json(new { success = false, message = "找不到寵物或錢包" });
            }

            var requiredExp = CalculateRequiredExp(pet.Level);
            if (pet.Experience >= requiredExp)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 升級
                    pet.Level++;
                    pet.Experience -= requiredExp;
                    pet.LevelUpTime = DateTime.UtcNow;

                    // 計算升級獎勵點數
                    var pointsReward = CalculateLevelUpReward(pet.Level);
                    pet.PointsGainedLevelUp = pointsReward;
                    pet.PointsGainedTimeLevelUp = DateTime.UtcNow;

                    // 增加用戶點數
                    userWallet.UserPoint += pointsReward;

                    // 記錄錢包歷史
                    _context.WalletHistories.Add(new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "Point",
                        PointsChanged = pointsReward,
                        Description = "寵物升級獎勵",
                        ChangeTime = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { 
                        success = true, 
                        message = $"恭喜！寵物升級到 Lv.{pet.Level}，獲得 {pointsReward} 點獎勵！",
                        level = pet.Level,
                        experience = pet.Experience,
                        points = userWallet.UserPoint
                    });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "升級失敗" });
                }
            }

            return Json(new { success = false, message = "經驗值不足，無法升級" });
        }

        private async Task<Pet> CreatePetForUser(int userId)
        {
            var pet = new Pet
            {
                UserId = userId,
                PetName = "小可愛",
                Level = 1,
                Experience = 0,
                Hunger = 50,
                Mood = 50,
                Stamina = 50,
                Cleanliness = 50,
                Health = 50,
                SkinColor = "#ADD8E6",
                BackgroundColor = "粉藍",
                LevelUpTime = DateTime.UtcNow,
                ColorChangedTime = DateTime.UtcNow,
                BackgroundColorChangedTime = DateTime.UtcNow,
                PointsChangedColor = 0,
                PointsChangedTimeColor = DateTime.UtcNow,
                PointsGainedLevelUp = 0,
                PointsGainedTimeLevelUp = DateTime.UtcNow
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return pet;
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
                return Math.Min(level * 60, 3000); // 上限 3000 點
        }

        private int GetCurrentUserId()
        {
            // 暫時返回固定用戶ID，實際應該從認證中獲取
            return 1;
        }
    }
}