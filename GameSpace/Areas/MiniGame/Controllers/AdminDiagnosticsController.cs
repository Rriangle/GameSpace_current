using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Reflection;
using System.Text.RegularExpressions;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area Admin 診斷控制器
    /// 提供欄位覆蓋率診斷等管理工具
    /// 基於 Pet_Rights_Management 權限控制存取
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminOnly]
    public class AdminDiagnosticsController : Controller
    {
        private readonly IHostEnvironment _environment;

        public AdminDiagnosticsController(IHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// 欄位覆蓋率診斷端點
        /// 路由：GET /MiniGame/AdminDiagnostics/FieldCoverage
        /// 驗證 Admin 頁面欄位覆蓋率與 database.json 對應
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> FieldCoverage()
        {
            try
            {
                var schemaPath = Path.Combine(_environment.ContentRootPath, "schema", "database.json");
                var schemaJson = await System.IO.File.ReadAllTextAsync(schemaPath);
                var schemaDoc = JsonDocument.Parse(schemaJson);

                var targetTables = new[]
                {
                    "User_Wallet", "CouponType", "Coupon", "EVoucherType", "EVoucher",
                    "EVoucherToken", "EVoucherRedeemLog", "WalletHistory", "UserSignInStats",
                    "Pet", "MiniGame"
                };

                var results = new List<object>();

                foreach (var tableName in targetTables)
                {
                    var schemaFields = GetSchemaFields(schemaDoc, tableName);
                    var entityFields = GetEntityFields(tableName);
                    var viewFields = await GetViewFields(tableName);

                    var missingInEntity = schemaFields.Except(entityFields).ToList();
                    var missingInView = schemaFields.Except(viewFields).ToList();

                    results.Add(new
                    {
                        table = tableName,
                        schemaCount = schemaFields.Count,
                        entityCount = entityFields.Count,
                        viewFieldCount = viewFields.Count,
                        missingInEntity = missingInEntity,
                        missingInView = missingInView,
                        notes = GenerateNotes(tableName, missingInEntity.Count, missingInView.Count)
                    });
                }

                return Json(new
                {
                    area = "MiniGame",
                    timestamp = DateTime.UtcNow,
                    total_tables = targetTables.Length,
                    summary = new
                    {
                        avg_entity_coverage = results.Average(r => (double)((dynamic)r).entityCount / ((dynamic)r).schemaCount * 100),
                        avg_view_coverage = results.Average(r => (double)((dynamic)r).viewFieldCount / ((dynamic)r).schemaCount * 100),
                        critical_gaps = results.Count(r => ((dynamic)r).missingInView.Count > 3)
                    },
                    tables = results
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = "診斷失敗",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        private List<string> GetSchemaFields(JsonDocument schemaDoc, string tableName)
        {
            var tables = schemaDoc.RootElement.GetProperty("tables").EnumerateArray();
            var table = tables.FirstOrDefault(t => t.GetProperty("name").GetString() == tableName);
            
            if (table.ValueKind == JsonValueKind.Undefined)
                return new List<string>();

            return table.GetProperty("columns")
                .EnumerateArray()
                .Select(c => c.GetProperty("name").GetString()!)
                .ToList();
        }

        private List<string> GetEntityFields(string tableName)
        {
            var entityType = tableName switch
            {
                "User_Wallet" => typeof(UserWallet),
                "CouponType" => typeof(CouponType),
                "Coupon" => typeof(Coupon),
                "EVoucherType" => typeof(EVoucherType),
                "EVoucher" => typeof(EVoucher),
                "EVoucherToken" => typeof(EVoucherToken),
                "EVoucherRedeemLog" => typeof(EVoucherRedeemLog),
                "WalletHistory" => typeof(WalletHistory),
                "UserSignInStats" => typeof(UserSignInStats),
                "Pet" => typeof(Pet),
                "MiniGame" => typeof(MiniGame),
                _ => null
            };

            if (entityType == null) return new List<string>();

            return entityType.GetProperties()
                .Where(p => !p.PropertyType.IsClass || p.PropertyType == typeof(string) || p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                .Select(p =>
                {
                    var columnAttr = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
                    return columnAttr?.Name ?? p.Name;
                })
                .ToList();
        }

        private async Task<List<string>> GetViewFields(string tableName)
        {
            var viewPaths = tableName switch
            {
                "User_Wallet" => new[] { "AdminWallet/Index.cshtml", "AdminWallet/Details.cshtml" },
                "WalletHistory" => new[] { "AdminWallet/History.cshtml" },
                "Coupon" => new[] { "AdminWallet/Coupons.cshtml" },
                "EVoucher" => new[] { "AdminWallet/EVouchers.cshtml" },
                "EVoucherToken" => new[] { "AdminWallet/Tokens.cshtml", "AdminWallet/EVoucherTokens.cshtml" },
                "EVoucherRedeemLog" => new[] { "AdminWallet/RedeemLogs.cshtml", "AdminWallet/EVoucherRedeemLogs.cshtml" },
                "UserSignInStats" => new[] { "AdminSignInStats/Index.cshtml", "AdminSignInStats/Statistics.cshtml" },
                "Pet" => new[] { "AdminPet/Index.cshtml", "AdminPet/Details.cshtml" },
                "MiniGame" => new[] { "AdminMiniGame/Index.cshtml", "AdminMiniGame/Statistics.cshtml" },
                _ => Array.Empty<string>()
            };

            var fields = new HashSet<string>();
            var basePath = Path.Combine(_environment.ContentRootPath, "Areas", "MiniGame", "Views");

            foreach (var viewPath in viewPaths)
            {
                var fullPath = Path.Combine(basePath, viewPath);
                if (System.IO.File.Exists(fullPath))
                {
                    var content = await System.IO.File.ReadAllTextAsync(fullPath);
                    var matches = Regex.Matches(content, @"@(?:Model|item|stat|game|pet|wallet|history|token|log)\.(\w+)", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        fields.Add(match.Groups[1].Value);
                    }
                }
            }

            return fields.ToList();
        }

        private string GenerateNotes(string tableName, int missingInEntity, int missingInView)
        {
            if (missingInEntity > 0 && missingInView > 0)
                return $"{tableName} 需要新增 Entity 屬性和 View 欄位";
            if (missingInEntity > 0)
                return $"{tableName} 需要新增 Entity 屬性對應";
            if (missingInView > 3)
                return $"{tableName} View 欄位覆蓋率較低，建議檢查";
            if (missingInView > 0)
                return $"{tableName} 有少數欄位未在 View 中顯示";
            return $"{tableName} 欄位覆蓋率良好";
        }
    }
}