using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using GameSpace.Data;
using GameSpace.Areas.MiniGame.Filters;
using GameSpace.Areas.MiniGame.Services;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area Admin 分析控制器
    /// 提供圖表資料的 JSON 端點，支援快取和超時處理
    /// 基於 Pet_Rights_Management 權限控制存取
    /// </summary>
    [Area("MiniGame")]
    [Authorize(Roles = "Admin")]
    [MiniGameAdminAuthorize]
    [TypeFilter(typeof(MiniGameProblemDetailsFilter))]
    public class AdminAnalyticsController : Controller
    {
        private readonly GameSpaceDbContext _context;
        private readonly MiniGameCache _cache;

        public AdminAnalyticsController(GameSpaceDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = new MiniGameCache(memoryCache);
        }

        /// <summary>
        /// 小遊戲總覽分析資料（支援快取和超時）
        /// 路由：GET /MiniGame/AdminAnalytics/MiniGameOverview
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MiniGameOverview(DateTime? from = null, DateTime? to = null, string? cache = null)
        {
            // 檢查是否略過快取
            var bypassCache = cache == "off";
            var cacheKey = MiniGameCache.MakeKey(Request);
            
            var result = await _cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(5),
                async (ct) =>
                {
                    // 設定查詢超時
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(2));

                    // 預設最近 14 天
                    var fromDate = from ?? DateTime.UtcNow.AddDays(-14);
                    var toDate = to ?? DateTime.UtcNow;
                    var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

                    var query = _context.MiniGames
                        .Where(m => m.PointsGainedTime >= fromDate && m.PointsGainedTime <= toDateEndOfDay)
                        .AsNoTracking();

                    var dailyAggregates = await query
                        .GroupBy(m => m.PointsGainedTime.Date)
                        .Select(g => new
                        {
                            date = g.Key.ToString("yyyy-MM-dd"),
                            sessions = g.Count(),
                            pointsSum = g.Sum(m => m.PointsGained),
                            expSum = g.Sum(m => m.ExpGained),
                            couponCount = g.Count(m => !string.IsNullOrEmpty(m.CouponGained) && m.CouponGained != "0")
                        })
                        .OrderBy(x => x.date)
                        .AsNoTracking()
                        .ToListAsync(cts.Token);

                    return new
                    {
                        status = "ok",
                        range = new
                        {
                            from = fromDate.ToString("yyyy-MM-dd"),
                            to = toDate.ToString("yyyy-MM-dd")
                        },
                        series = dailyAggregates,
                        cached = false,
                        timestamp = DateTime.UtcNow
                    };
                },
                HttpContext.RequestAborted,
                bypassCache
            );

            // 標記是否來自快取
            var responseWithCacheInfo = new
            {
                result.status,
                result.range,
                result.series,
                cached = !bypassCache && result.cached,
                timestamp = result.timestamp
            };

            return Json(responseWithCacheInfo);
        }

        /// <summary>
        /// 簽到總覽分析資料（支援快取和超時）
        /// 路由：GET /MiniGame/AdminAnalytics/SignInOverview
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SignInOverview(DateTime? from = null, DateTime? to = null, string? cache = null)
        {
            // 檢查是否略過快取
            var bypassCache = cache == "off";
            var cacheKey = MiniGameCache.MakeKey(Request);
            
            var result = await _cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(5),
                async (ct) =>
                {
                    // 設定查詢超時
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(2));

                    // 預設最近 14 天
                    var fromDate = from ?? DateTime.UtcNow.AddDays(-14);
                    var toDate = to ?? DateTime.UtcNow;

                    var query = _context.UserSignInStats
                        .Where(s => s.SignTime.Date >= fromDate.Date && s.SignTime.Date <= toDate.Date)
                        .AsNoTracking();

                    var dailyAggregates = await query
                        .GroupBy(s => s.SignTime.Date)
                        .Select(g => new
                        {
                            date = g.Key.ToString("yyyy-MM-dd"),
                            signInCount = g.Count(),
                            rewardPointsSum = g.Sum(s => s.PointsGained),
                            rewardExpSum = g.Sum(s => s.ExpGained),
                            pointsGainedSum = g.Sum(s => s.PointsGained)
                        })
                        .OrderBy(x => x.date)
                        .AsNoTracking()
                        .ToListAsync(cts.Token);

                    return new
                    {
                        status = "ok",
                        range = new
                        {
                            from = fromDate.ToString("yyyy-MM-dd"),
                            to = toDate.ToString("yyyy-MM-dd")
                        },
                        series = dailyAggregates,
                        cached = false,
                        timestamp = DateTime.UtcNow
                    };
                },
                HttpContext.RequestAborted,
                bypassCache
            );

            return Json(result);
        }

        /// <summary>
        /// 錢包歷史每日分析資料（支援快取和超時）
        /// 路由：GET /MiniGame/AdminAnalytics/WalletHistoryDaily
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> WalletHistoryDaily(DateTime? from = null, DateTime? to = null, string? cache = null)
        {
            // 檢查是否略過快取
            var bypassCache = cache == "off";
            var cacheKey = MiniGameCache.MakeKey(Request);
            
            var result = await _cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(5),
                async (ct) =>
                {
                    // 設定查詢超時
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(2));

                    // 預設最近 14 天
                    var fromDate = from ?? DateTime.UtcNow.AddDays(-14);
                    var toDate = to ?? DateTime.UtcNow;
                    var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

                    var query = _context.WalletHistories
                        .Where(h => h.ChangeTime >= fromDate && h.ChangeTime <= toDateEndOfDay)
                        .AsNoTracking();

                    var dailyAggregates = await query
                        .GroupBy(h => h.ChangeTime.Date)
                        .Select(g => new
                        {
                            date = g.Key.ToString("yyyy-MM-dd"),
                            count = g.Count(),
                            pointsSum = g.Sum(h => h.PointsChanged)
                        })
                        .OrderBy(x => x.date)
                        .AsNoTracking()
                        .ToListAsync(cts.Token);

                    return new
                    {
                        status = "ok",
                        range = new
                        {
                            from = fromDate.ToString("yyyy-MM-dd"),
                            to = toDate.ToString("yyyy-MM-dd")
                        },
                        series = dailyAggregates,
                        cached = false,
                        timestamp = DateTime.UtcNow
                    };
                },
                HttpContext.RequestAborted,
                bypassCache
            );

            return Json(result);
        }

        /// <summary>
        /// 兌換記錄每日分析資料（支援快取和超時）
        /// 路由：GET /MiniGame/AdminAnalytics/RedeemLogsDaily
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RedeemLogsDaily(DateTime? from = null, DateTime? to = null, string? status = null, string? cache = null)
        {
            // 檢查是否略過快取
            var bypassCache = cache == "off";
            var cacheKey = MiniGameCache.MakeKey(Request);
            
            var result = await _cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(5),
                async (ct) =>
                {
                    // 設定查詢超時
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(2));

                    // 預設最近 14 天
                    var fromDate = from ?? DateTime.UtcNow.AddDays(-14);
                    var toDate = to ?? DateTime.UtcNow;
                    var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

                    var query = _context.EVoucherRedeemLogs
                        .Where(r => r.ScannedAt >= fromDate && r.ScannedAt <= toDateEndOfDay)
                        .AsNoTracking();

                    // 如果提供狀態篩選，則套用；否則計算所有
                    if (!string.IsNullOrEmpty(status))
                    {
                        query = query.Where(r => r.Status == status);
                    }

                    var dailyAggregates = await query
                        .GroupBy(r => r.ScannedAt.Date)
                        .Select(g => new
                        {
                            date = g.Key.ToString("yyyy-MM-dd"),
                            count = g.Count()
                        })
                        .OrderBy(x => x.date)
                        .AsNoTracking()
                        .ToListAsync(cts.Token);

                    return new
                    {
                        status = "ok",
                        range = new
                        {
                            from = fromDate.ToString("yyyy-MM-dd"),
                            to = toDate.ToString("yyyy-MM-dd")
                        },
                        filter = new
                        {
                            status = status ?? "all"
                        },
                        series = dailyAggregates,
                        cached = false,
                        timestamp = DateTime.UtcNow
                    };
                },
                HttpContext.RequestAborted,
                bypassCache
            );

            return Json(result);
        }

        /// <summary>
        /// Token 每日分析資料（支援快取和超時）
        /// 路由：GET /MiniGame/AdminAnalytics/TokensDaily
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TokensDaily(DateTime? from = null, DateTime? to = null, string? cache = null)
        {
            // 檢查是否略過快取
            var bypassCache = cache == "off";
            var cacheKey = MiniGameCache.MakeKey(Request);
            
            var result = await _cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(5),
                async (ct) =>
                {
                    // 設定查詢超時
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(2));

                    // 預設最近 14 天
                    var fromDate = from ?? DateTime.UtcNow.AddDays(-14);
                    var toDate = to ?? DateTime.UtcNow;
                    var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

                    var query = _context.EVoucherTokens
                        .Where(t => t.ExpiresAt >= fromDate && t.ExpiresAt <= toDateEndOfDay)
                        .AsNoTracking();

                    var dailyAggregates = await query
                        .GroupBy(t => t.ExpiresAt.Date)
                        .Select(g => new
                        {
                            date = g.Key.ToString("yyyy-MM-dd"),
                            count = g.Count()
                        })
                        .OrderBy(x => x.date)
                        .AsNoTracking()
                        .ToListAsync(cts.Token);

                    return new
                    {
                        status = "ok",
                        range = new
                        {
                            from = fromDate.ToString("yyyy-MM-dd"),
                            to = toDate.ToString("yyyy-MM-dd")
                        },
                        series = dailyAggregates,
                        cached = false,
                        timestamp = DateTime.UtcNow
                    };
                },
                HttpContext.RequestAborted,
                bypassCache
            );

            return Json(result);
        }

        /// <summary>
        /// Coupon 每日分析資料（支援快取和超時）
        /// 路由：GET /MiniGame/AdminAnalytics/CouponsDaily
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CouponsDaily(DateTime? from = null, DateTime? to = null, string? cache = null)
        {
            // 檢查是否略過快取
            var bypassCache = cache == "off";
            var cacheKey = MiniGameCache.MakeKey(Request);
            
            var result = await _cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(5),
                async (ct) =>
                {
                    // 設定查詢超時
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(2));

                    // 預設最近 14 天
                    var fromDate = from ?? DateTime.UtcNow.AddDays(-14);
                    var toDate = to ?? DateTime.UtcNow;
                    var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

                    var query = _context.Coupons
                        .Where(c => c.AcquiredTime >= fromDate && c.AcquiredTime <= toDateEndOfDay)
                        .AsNoTracking();

                    var dailyAggregates = await query
                        .GroupBy(c => c.AcquiredTime.Date)
                        .Select(g => new
                        {
                            date = g.Key.ToString("yyyy-MM-dd"),
                            count = g.Count()
                        })
                        .OrderBy(x => x.date)
                        .AsNoTracking()
                        .ToListAsync(cts.Token);

                    return new
                    {
                        status = "ok",
                        range = new
                        {
                            from = fromDate.ToString("yyyy-MM-dd"),
                            to = toDate.ToString("yyyy-MM-dd")
                        },
                        series = dailyAggregates,
                        cached = false,
                        timestamp = DateTime.UtcNow
                    };
                },
                HttpContext.RequestAborted,
                bypassCache
            );

            return Json(result);
        }

        /// <summary>
        /// EVoucher 每日分析資料（支援快取和超時）
        /// 路由：GET /MiniGame/AdminAnalytics/EVouchersDaily
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EVouchersDaily(DateTime? from = null, DateTime? to = null, string? cache = null)
        {
            // 檢查是否略過快取
            var bypassCache = cache == "off";
            var cacheKey = MiniGameCache.MakeKey(Request);
            
            var result = await _cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(5),
                async (ct) =>
                {
                    // 設定查詢超時
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(2));

                    // 預設最近 14 天
                    var fromDate = from ?? DateTime.UtcNow.AddDays(-14);
                    var toDate = to ?? DateTime.UtcNow;
                    var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

                    var query = _context.EVouchers
                        .Where(e => e.AcquiredTime >= fromDate && e.AcquiredTime <= toDateEndOfDay)
                        .AsNoTracking();

                    var dailyAggregates = await query
                        .GroupBy(e => e.AcquiredTime.Date)
                        .Select(g => new
                        {
                            date = g.Key.ToString("yyyy-MM-dd"),
                            count = g.Count()
                        })
                        .OrderBy(x => x.date)
                        .AsNoTracking()
                        .ToListAsync(cts.Token);

                    return new
                    {
                        status = "ok",
                        range = new
                        {
                            from = fromDate.ToString("yyyy-MM-dd"),
                            to = toDate.ToString("yyyy-MM-dd")
                        },
                        series = dailyAggregates,
                        cached = false,
                        timestamp = DateTime.UtcNow
                    };
                },
                HttpContext.RequestAborted,
                bypassCache
            );

            return Json(result);
        }

        /// <summary>
        /// 用戶錢包點數分佈分析（支援快取和超時）
        /// 路由：GET /MiniGame/AdminAnalytics/UserWalletBuckets
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UserWalletBuckets(string? thresholds = "100,500,1000", string? cache = null)
        {
            // 檢查是否略過快取
            var bypassCache = cache == "off";
            var cacheKey = MiniGameCache.MakeKey(Request);
            
            var result = await _cache.GetOrCreateAsync(
                cacheKey,
                TimeSpan.FromMinutes(5),
                async (ct) =>
                {
                    // 設定查詢超時
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    cts.CancelAfter(TimeSpan.FromSeconds(2));

                    // 解析閾值
                    var thresholdValues = thresholds?.Split(',')
                        .Select(t => int.TryParse(t.Trim(), out var val) ? val : 0)
                        .Where(t => t > 0)
                        .OrderBy(t => t)
                        .ToArray() ?? new[] { 100, 500, 1000 };

                    var query = _context.UserWallets
                        .AsNoTracking();

                    var allWallets = await query
                        .Select(w => w.CurrentPoints)
                        .AsNoTracking()
                        .ToListAsync(cts.Token);

                    // 建立分組
                    var buckets = new List<object>();
                    var previousThreshold = 0;

                    foreach (var threshold in thresholdValues)
                    {
                        var count = allWallets.Count(points => points >= previousThreshold && points < threshold);
                        buckets.Add(new
                        {
                            label = $"{previousThreshold}~{threshold - 1}",
                            min = previousThreshold,
                            max = threshold - 1,
                            count = count
                        });
                        previousThreshold = threshold;
                    }

                    // 最後一個分組（≥最高閾值）
                    var maxCount = allWallets.Count(points => points >= previousThreshold);
                    buckets.Add(new
                    {
                        label = $"≥{previousThreshold}",
                        min = previousThreshold,
                        max = int.MaxValue,
                        count = maxCount
                    });

                    return new
                    {
                        status = "ok",
                        thresholds = thresholdValues,
                        totalUsers = allWallets.Count,
                        buckets = buckets,
                        cached = false,
                        timestamp = DateTime.UtcNow
                    };
                },
                HttpContext.RequestAborted,
                bypassCache
            );

            return Json(result);
        }

        /// <summary>
        /// 快取失效端點（僅限管理員）
        /// 路由：GET /MiniGame/AdminAnalytics/Cache/Invalidate
        /// </summary>
        [HttpGet("Cache/Invalidate")]
        public IActionResult CacheInvalidate()
        {
            try
            {
                var statsBefore = _cache.GetCacheStats();
                _cache.InvalidateAll();
                var statsAfter = _cache.GetCacheStats();

                return Json(new
                {
                    status = "ok",
                    message = "快取已清除",
                    before = statsBefore,
                    after = statsAfter,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = $"快取清除失敗：{ex.Message}",
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}