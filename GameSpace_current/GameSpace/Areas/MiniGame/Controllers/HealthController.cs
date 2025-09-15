using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area 健康檢查控制器
    /// 提供資料庫連線檢查端點，確保 MiniGame Admin 系統正常運作
    /// </summary>
    [Area("MiniGame")]
    public class HealthController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public HealthController(GameSpaceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 資料庫健康檢查端點
        /// 路由：/MiniGame/Health/Database
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Database()
        {
            try
            {
                // 檢查資料庫連線
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return Json(new { 
                        status = "error", 
                        message = "無法連接資料庫",
                        timestamp = DateTime.UtcNow 
                    });
                }

                // 檢查 MiniGame Area 相關資料表
                var tableChecks = new Dictionary<string, bool>();
                
                try
                {
                    tableChecks["User_Wallet"] = await _context.UserWallets.AnyAsync();
                }
                catch
                {
                    tableChecks["User_Wallet"] = false;
                }

                try
                {
                    tableChecks["CouponType"] = await _context.CouponTypes.AnyAsync();
                }
                catch
                {
                    tableChecks["CouponType"] = false;
                }

                try
                {
                    tableChecks["EVoucherType"] = await _context.EVoucherTypes.AnyAsync();
                }
                catch
                {
                    tableChecks["EVoucherType"] = false;
                }

                try
                {
                    tableChecks["UserSignInStats"] = await _context.UserSignInStats.AnyAsync();
                }
                catch
                {
                    tableChecks["UserSignInStats"] = false;
                }

                try
                {
                    tableChecks["Pet"] = await _context.Pets.AnyAsync();
                }
                catch
                {
                    tableChecks["Pet"] = false;
                }

                try
                {
                    tableChecks["MiniGame"] = await _context.MiniGames.AnyAsync();
                }
                catch
                {
                    tableChecks["MiniGame"] = false;
                }

                var allTablesAccessible = tableChecks.Values.All(v => v);

                return Json(new { 
                    status = allTablesAccessible ? "ok" : "warning",
                    message = allTablesAccessible ? "MiniGame Area 資料庫健康檢查通過" : "部分資料表無法存取",
                    database_connected = canConnect,
                    tables = tableChecks,
                    timestamp = DateTime.UtcNow,
                    area = "MiniGame",
                    modules = new[] { "User_Wallet", "UserSignInStats", "Pet", "MiniGame" }
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    status = "error", 
                    message = $"健康檢查失敗：{ex.Message}",
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// MiniGame Admin 系統狀態檢查
        /// 路由：/MiniGame/Health/Status
        /// </summary>
        [HttpGet]
        public IActionResult Status()
        {
            return Json(new
            {
                status = "ok",
                message = "MiniGame Admin 系統運行正常",
                version = "1.0.0",
                area = "MiniGame",
                modules = new
                {
                    User_Wallet = new
                    {
                        controllers = new[] { "AdminWalletController", "AdminWalletTypesController" },
                        tables = new[] { "User_Wallet", "CouponType", "Coupon", "EVoucherType", "EVoucher", "EVoucherToken", "EVoucherRedeemLog", "WalletHistory" },
                        features = new[] { "Read-first查詢", "型別表CRUD", "錢包管理", "券類管理" }
                    },
                    UserSignInStats = new
                    {
                        controllers = new[] { "AdminSignInStatsController" },
                        tables = new[] { "UserSignInStats" },
                        features = new[] { "Read-first查詢", "簽到統計", "歷史記錄" }
                    },
                    Pet = new
                    {
                        controllers = new[] { "AdminPetController" },
                        tables = new[] { "Pet" },
                        features = new[] { "Read-first查詢", "狀態調整預留實作" }
                    },
                    MiniGame = new
                    {
                        controllers = new[] { "AdminMiniGameController" },
                        tables = new[] { "MiniGame" },
                        features = new[] { "Read-first查詢", "設定管理預留實作" }
                    }
                },
                compliance = new
                {
                    area_boundary = "嚴格遵循 MiniGame Area 邊界",
                    ui_style = "SB Admin 風格",
                    read_first = "優先完成查閱與篩選功能",
                    crud_limitation = "僅型別表提供 CRUD",
                    reserved_approach = "其餘表為審閱頁或不破壞規格的預留實作",
                    language = "所有人類可讀輸出皆為 zh-TW"
                },
                timestamp = DateTime.UtcNow
            });
        }
    }
}