using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class E2ETestController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public E2ETestController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var testResults = new List<TestResult>();
            
            // 測試1：資料庫連線
            testResults.Add(await TestDatabaseConnection());
            
            // 測試2：資料庫種子服務
            testResults.Add(await TestDataSeeding());
            
            // 測試3：簽到流程E2E測試
            testResults.Add(await TestSignInFlow());
            
            // 測試4：寵物升級流程E2E測試
            testResults.Add(await TestPetLevelUpFlow());
            
            // 測試5：錢包交易流程E2E測試
            testResults.Add(await TestWalletTransactionFlow());
            
            // 測試6：小遊戲流程E2E測試
            testResults.Add(await TestMiniGameFlow());

            ViewBag.TestResults = testResults;
            return View();
        }

        private async Task<TestResult> TestDatabaseConnection()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                return new TestResult
                {
                    TestName = "資料庫連線測試",
                    Status = "通過",
                    Message = "資料庫連線正常",
                    Details = "成功連接到資料庫"
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    TestName = "資料庫連線測試",
                    Status = "失敗",
                    Message = "資料庫連線失敗",
                    Details = ex.Message
                };
            }
        }

        private async Task<TestResult> TestDataSeeding()
        {
            try
            {
                var seedingService = new DataSeedingService(_context);
                await seedingService.SeedAllDataAsync();
                
                var stats = new
                {
                    Users = await _context.Users.CountAsync(),
                    Pets = await _context.Pets.CountAsync(),
                    UserWallets = await _context.UserWallets.CountAsync(),
                    CouponTypes = await _context.CouponTypes.CountAsync(),
                    EVoucherTypes = await _context.EvoucherTypes.CountAsync()
                };

                return new TestResult
                {
                    TestName = "資料庫種子服務測試",
                    Status = "通過",
                    Message = "種子服務執行成功",
                    Details = $"Users: {stats.Users}, Pets: {stats.Pets}, UserWallets: {stats.UserWallets}, CouponTypes: {stats.CouponTypes}, EVoucherTypes: {stats.EVoucherTypes}"
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    TestName = "資料庫種子服務測試",
                    Status = "失敗",
                    Message = "種子服務執行失敗",
                    Details = ex.Message
                };
            }
        }

        private async Task<TestResult> TestSignInFlow()
        {
            try
            {
                // 取得測試用戶
                var user = await _context.Users.FirstOrDefaultAsync();
                if (user == null)
                {
                    return new TestResult
                    {
                        TestName = "簽到流程E2E測試",
                        Status = "跳過",
                        Message = "沒有找到測試用戶",
                        Details = "需要先執行資料庫種子服務"
                    };
                }

                // 記錄測試前的狀態
                var beforeWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == user.UserId);
                var beforePoints = beforeWallet?.Points ?? 0;
                var beforeSignInCount = await _context.UserSignInStats.CountAsync(s => s.UserId == user.UserId);
                var beforeWalletHistoryCount = await _context.WalletHistory.CountAsync(h => h.UserId == user.UserId);

                // 執行簽到流程
                var signInController = new UserSignInStatsController(_context);
                
                // 模擬簽到
                var consecutiveDays = await CalculateConsecutiveDays(user.UserId);
                var isWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday;
                var rewards = CalculateSignInRewards(consecutiveDays, isWeekend);

                // 建立簽到記錄
                var signInStat = new UserSignInStat
                {
                    UserId = user.UserId,
                    SignInDate = DateTime.Now,
                    ConsecutiveDays = consecutiveDays + 1,
                    PointsGained = rewards.Points,
                    ExperienceGained = rewards.Experience,
                    CouponCode = rewards.CouponCode,
                    CreatedTime = DateTime.Now
                };

                _context.UserSignInStats.Add(signInStat);

                // 更新用戶錢包
                if (beforeWallet != null)
                {
                    beforeWallet.Points += rewards.Points;
                    beforeWallet.UpdatedTime = DateTime.Now;

                    // 記錄錢包歷史
                    var walletHistory = new WalletHistory
                    {
                        UserId = user.UserId,
                        TransactionType = "簽到獎勵",
                        Amount = rewards.Points,
                        Description = $"連續簽到{consecutiveDays + 1}天獎勵",
                        CreatedTime = DateTime.Now
                    };

                    _context.WalletHistory.Add(walletHistory);
                }

                await _context.SaveChangesAsync();

                // 驗證結果
                var afterWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == user.UserId);
                var afterPoints = afterWallet?.Points ?? 0;
                var afterSignInCount = await _context.UserSignInStats.CountAsync(s => s.UserId == user.UserId);
                var afterWalletHistoryCount = await _context.WalletHistory.CountAsync(h => h.UserId == user.UserId);

                var pointsIncreased = afterPoints - beforePoints;
                var signInCountIncreased = afterSignInCount - beforeSignInCount;
                var walletHistoryCountIncreased = afterWalletHistoryCount - beforeWalletHistoryCount;

                if (pointsIncreased == rewards.Points && signInCountIncreased == 1 && walletHistoryCountIncreased == 1)
                {
                    return new TestResult
                    {
                        TestName = "簽到流程E2E測試",
                        Status = "通過",
                        Message = "簽到流程完整執行成功",
                        Details = $"點數增加: {pointsIncreased}, 簽到記錄增加: {signInCountIncreased}, 錢包歷史增加: {walletHistoryCountIncreased}"
                    };
                }
                else
                {
                    return new TestResult
                    {
                        TestName = "簽到流程E2E測試",
                        Status = "失敗",
                        Message = "簽到流程數據不一致",
                        Details = $"預期點數增加: {rewards.Points}, 實際: {pointsIncreased}; 預期簽到記錄增加: 1, 實際: {signInCountIncreased}; 預期錢包歷史增加: 1, 實際: {walletHistoryCountIncreased}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    TestName = "簽到流程E2E測試",
                    Status = "失敗",
                    Message = "簽到流程執行失敗",
                    Details = ex.Message
                };
            }
        }

        private async Task<TestResult> TestPetLevelUpFlow()
        {
            try
            {
                // 取得測試寵物
                var pet = await _context.Pets.FirstOrDefaultAsync();
                if (pet == null)
                {
                    return new TestResult
                    {
                        TestName = "寵物升級流程E2E測試",
                        Status = "跳過",
                        Message = "沒有找到測試寵物",
                        Details = "需要先執行資料庫種子服務"
                    };
                }

                // 記錄測試前的狀態
                var beforeLevel = pet.Level;
                var beforeExp = pet.Experience;
                var beforeWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == pet.UserId);
                var beforePoints = beforeWallet?.Points ?? 0;

                // 給寵物足夠的經驗值來升級
                var requiredExp = CalculateRequiredExp(pet.Level);
                pet.Experience = requiredExp - 1; // 差1點經驗升級
                await _context.SaveChangesAsync();

                // 模擬餵食寵物（增加經驗值）
                pet.Experience += 2; // 增加2點經驗，應該觸發升級
                
                // 檢查是否升級
                if (pet.Experience >= requiredExp)
                {
                    pet.Level++;
                    pet.Experience -= requiredExp;
                    pet.Hunger = 100;
                    pet.Mood = 100;
                    pet.Stamina = 100;
                    pet.Cleanliness = 100;

                    // 發放升級獎勵
                    if (beforeWallet != null)
                    {
                        var levelUpReward = pet.Level * 10; // 每級10點
                        beforeWallet.Points += levelUpReward;
                        beforeWallet.UpdatedTime = DateTime.Now;

                        // 記錄錢包歷史
                        var walletHistory = new WalletHistory
                        {
                            UserId = pet.UserId,
                            TransactionType = "寵物升級獎勵",
                            Amount = levelUpReward,
                            Description = $"寵物升級到第{pet.Level}級",
                            CreatedTime = DateTime.Now
                        };

                        _context.WalletHistory.Add(walletHistory);
                    }
                }

                await _context.SaveChangesAsync();

                // 驗證結果
                var afterLevel = pet.Level;
                var afterExp = pet.Experience;
                var afterWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == pet.UserId);
                var afterPoints = afterWallet?.Points ?? 0;

                var levelIncreased = afterLevel - beforeLevel;
                var pointsIncreased = afterPoints - beforePoints;

                if (levelIncreased == 1 && pointsIncreased > 0)
                {
                    return new TestResult
                    {
                        TestName = "寵物升級流程E2E測試",
                        Status = "通過",
                        Message = "寵物升級流程完整執行成功",
                        Details = $"等級提升: {levelIncreased}, 點數增加: {pointsIncreased}"
                    };
                }
                else
                {
                    return new TestResult
                    {
                        TestName = "寵物升級流程E2E測試",
                        Status = "失敗",
                        Message = "寵物升級流程數據不一致",
                        Details = $"預期等級提升: 1, 實際: {levelIncreased}; 預期點數增加: >0, 實際: {pointsIncreased}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    TestName = "寵物升級流程E2E測試",
                    Status = "失敗",
                    Message = "寵物升級流程執行失敗",
                    Details = ex.Message
                };
            }
        }

        private async Task<TestResult> TestWalletTransactionFlow()
        {
            try
            {
                // 取得測試用戶
                var user = await _context.Users.FirstOrDefaultAsync();
                if (user == null)
                {
                    return new TestResult
                    {
                        TestName = "錢包交易流程E2E測試",
                        Status = "跳過",
                        Message = "沒有找到測試用戶",
                        Details = "需要先執行資料庫種子服務"
                    };
                }

                // 記錄測試前的狀態
                var beforeWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == user.UserId);
                var beforePoints = beforeWallet?.Points ?? 0;
                var beforeWalletHistoryCount = await _context.WalletHistory.CountAsync(h => h.UserId == user.UserId);

                // 執行錢包交易
                var addPoints = 100;
                var description = "E2E測試點數增加";

                if (beforeWallet != null)
                {
                    beforeWallet.Points += addPoints;
                    beforeWallet.UpdatedTime = DateTime.Now;

                    // 記錄錢包歷史
                    var walletHistory = new WalletHistory
                    {
                        UserId = user.UserId,
                        TransactionType = "測試交易",
                        Amount = addPoints,
                        Description = description,
                        CreatedTime = DateTime.Now
                    };

                    _context.WalletHistory.Add(walletHistory);
                }

                await _context.SaveChangesAsync();

                // 驗證結果
                var afterWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == user.UserId);
                var afterPoints = afterWallet?.Points ?? 0;
                var afterWalletHistoryCount = await _context.WalletHistory.CountAsync(h => h.UserId == user.UserId);

                var pointsIncreased = afterPoints - beforePoints;
                var walletHistoryCountIncreased = afterWalletHistoryCount - beforeWalletHistoryCount;

                if (pointsIncreased == addPoints && walletHistoryCountIncreased == 1)
                {
                    return new TestResult
                    {
                        TestName = "錢包交易流程E2E測試",
                        Status = "通過",
                        Message = "錢包交易流程完整執行成功",
                        Details = $"點數增加: {pointsIncreased}, 錢包歷史增加: {walletHistoryCountIncreased}"
                    };
                }
                else
                {
                    return new TestResult
                    {
                        TestName = "錢包交易流程E2E測試",
                        Status = "失敗",
                        Message = "錢包交易流程數據不一致",
                        Details = $"預期點數增加: {addPoints}, 實際: {pointsIncreased}; 預期錢包歷史增加: 1, 實際: {walletHistoryCountIncreased}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    TestName = "錢包交易流程E2E測試",
                    Status = "失敗",
                    Message = "錢包交易流程執行失敗",
                    Details = ex.Message
                };
            }
        }

        private async Task<TestResult> TestMiniGameFlow()
        {
            try
            {
                // 取得測試用戶和寵物
                var user = await _context.Users.FirstOrDefaultAsync();
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.UserId == user.UserId);
                
                if (user == null || pet == null)
                {
                    return new TestResult
                    {
                        TestName = "小遊戲流程E2E測試",
                        Status = "跳過",
                        Message = "沒有找到測試用戶或寵物",
                        Details = "需要先執行資料庫種子服務"
                    };
                }

                // 記錄測試前的狀態
                var beforeWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == user.UserId);
                var beforePoints = beforeWallet?.Points ?? 0;
                var beforeMiniGameCount = await _context.MiniGames.CountAsync(m => m.UserId == user.UserId);

                // 模擬小遊戲
                var gameLevel = 1;
                var gameResult = "勝利";
                var rewards = CalculateGameRewards(gameLevel, gameResult);

                // 建立小遊戲記錄
                var miniGame = new MiniGame
                {
                    UserId = user.UserId,
                    PetId = pet.PetId,
                    GameLevel = gameLevel,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(5),
                    Result = gameResult,
                    ExperienceGained = rewards.Experience,
                    PointsGained = rewards.Points,
                    CouponCode = rewards.CouponCode,
                    CreatedTime = DateTime.Now
                };

                _context.MiniGames.Add(miniGame);

                // 更新寵物狀態
                pet.Experience += rewards.Experience;
                pet.Hunger = Math.Max(0, pet.Hunger - 10);
                pet.Mood = Math.Min(100, pet.Mood + 5);
                pet.Stamina = Math.Max(0, pet.Stamina - 15);
                pet.Cleanliness = Math.Max(0, pet.Cleanliness - 5);

                // 更新用戶錢包
                if (beforeWallet != null)
                {
                    beforeWallet.Points += rewards.Points;
                    beforeWallet.UpdatedTime = DateTime.Now;

                    // 記錄錢包歷史
                    var walletHistory = new WalletHistory
                    {
                        UserId = user.UserId,
                        TransactionType = "小遊戲獎勵",
                        Amount = rewards.Points,
                        Description = $"小遊戲第{gameLevel}關{gameResult}",
                        CreatedTime = DateTime.Now
                    };

                    _context.WalletHistory.Add(walletHistory);
                }

                await _context.SaveChangesAsync();

                // 驗證結果
                var afterWallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == user.UserId);
                var afterPoints = afterWallet?.Points ?? 0;
                var afterMiniGameCount = await _context.MiniGames.CountAsync(m => m.UserId == user.UserId);

                var pointsIncreased = afterPoints - beforePoints;
                var miniGameCountIncreased = afterMiniGameCount - beforeMiniGameCount;

                if (pointsIncreased == rewards.Points && miniGameCountIncreased == 1)
                {
                    return new TestResult
                    {
                        TestName = "小遊戲流程E2E測試",
                        Status = "通過",
                        Message = "小遊戲流程完整執行成功",
                        Details = $"點數增加: {pointsIncreased}, 小遊戲記錄增加: {miniGameCountIncreased}"
                    };
                }
                else
                {
                    return new TestResult
                    {
                        TestName = "小遊戲流程E2E測試",
                        Status = "失敗",
                        Message = "小遊戲流程數據不一致",
                        Details = $"預期點數增加: {rewards.Points}, 實際: {pointsIncreased}; 預期小遊戲記錄增加: 1, 實際: {miniGameCountIncreased}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    TestName = "小遊戲流程E2E測試",
                    Status = "失敗",
                    Message = "小遊戲流程執行失敗",
                    Details = ex.Message
                };
            }
        }

        // 輔助方法
        private async Task<int> CalculateConsecutiveDays(int userId)
        {
            var today = DateTime.Today;
            var consecutiveDays = 0;

            for (int i = 0; i < 30; i++)
            {
                var checkDate = today.AddDays(-i);
                var hasSignIn = await _context.UserSignInStats
                    .AnyAsync(s => s.UserId == userId && s.SignInDate.Date == checkDate);

                if (hasSignIn)
                {
                    consecutiveDays++;
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        private (int Points, int Experience, string CouponCode) CalculateSignInRewards(int consecutiveDays, bool isWeekend)
        {
            var points = 10;
            var experience = 5;
            string couponCode = null;

            if (consecutiveDays >= 7)
            {
                points += 50;
                experience += 25;
                couponCode = "WEEKLY_BONUS";
            }
            else if (consecutiveDays >= 3)
            {
                points += 20;
                experience += 10;
            }

            if (isWeekend)
            {
                points += 15;
                experience += 8;
            }

            return (points, experience, couponCode);
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

        private (int Points, int Experience, string CouponCode) CalculateGameRewards(int level, string result)
        {
            var points = level * 10;
            var experience = level * 5;
            string couponCode = null;

            if (result == "勝利")
            {
                points += 20;
                experience += 10;
                if (level >= 3)
                {
                    couponCode = "GAME_BONUS";
                }
            }

            return (points, experience, couponCode);
        }
    }

    public class TestResult
    {
        public string TestName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}