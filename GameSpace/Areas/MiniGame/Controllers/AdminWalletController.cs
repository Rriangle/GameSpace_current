using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area - User_Wallet 模組後台管理控制器
    /// 負責管理會員錢包、點數、優惠券、電子禮券的後台功能
    /// 資料表範圍：User_Wallet, CouponType, Coupon, EVoucherType, EVoucher, EVoucherToken, EVoucherRedeemLog, WalletHistory
    /// 基於 Pet_Rights_Management 權限控制存取
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminOnly]
    public class AdminWalletController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public AdminWalletController(GameSpaceDbContext context)
        {
            _context = context;
        }

        #region User_Wallet 管理

        /// <summary>
        /// 錢包列表頁面 - Read-first 原則
        /// </summary>
        // [TRACE] see docs/traceability/minigame_traceability_matrix.md#adminwallet-index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string search = "")
        {
            var query = _context.UserWallets
                .Include(w => w.User)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(w => w.User.UserName.Contains(search) || 
                                        w.User.UserNickName.Contains(search));
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var wallets = await query
                .OrderByDescending(w => w.UserPoint)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(wallets);
        }

        /// <summary>
        /// 錢包明細頁面
        /// </summary>
        public async Task<IActionResult> Details(int userId)
        {
            var wallet = await _context.UserWallets
                .Include(w => w.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserID == userId);

            if (wallet == null)
            {
                return NotFound("找不到指定的錢包資料");
            }

            // 取得錢包歷史記錄
            var histories = await _context.WalletHistories
                .Where(h => h.UserID == userId)
                .OrderByDescending(h => h.ChangeTime)
                .Take(50)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Histories = histories;

            return View(wallet);
        }

        #endregion

        #region WalletHistory 管理

        /// <summary>
        /// 錢包歷史記錄列表
        /// </summary>
        public async Task<IActionResult> History(int page = 1, int pageSize = 50, int? userId = null, string changeType = "")
        {
            var query = _context.WalletHistories
                .Include(h => h.User)
                .AsNoTracking();

            if (userId.HasValue)
            {
                query = query.Where(h => h.UserID == userId.Value);
            }

            if (!string.IsNullOrEmpty(changeType))
            {
                query = query.Where(h => h.ChangeType == changeType);
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var histories = await query
                .OrderByDescending(h => h.ChangeTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.UserId = userId;
            ViewBag.ChangeType = changeType;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(histories);
        }

        /// <summary>
        /// 錢包歷史 CSV 匯出
        /// </summary>
        public async Task<IActionResult> HistoryExportCsv(int? userId = null, string? type = null, string? q = null, DateTime? from = null, DateTime? to = null)
        {
            var query = BuildHistoryQuery(userId, type, q, from, to);
            
            var histories = await query
                .OrderByDescending(h => h.ChangeTime)
                .ThenByDescending(h => h.LogID)
                .AsNoTracking()
                .ToListAsync();

            var csv = GenerateHistoryCsv(histories);
            var fileName = $"wallet_history_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        /// <summary>
        /// 錢包歷史 JSON 匯出
        /// </summary>
        public async Task<IActionResult> HistoryExportJson(int? userId = null, string? type = null, string? q = null, DateTime? from = null, DateTime? to = null)
        {
            var query = BuildHistoryQuery(userId, type, q, from, to);
            
            var histories = await query
                .OrderByDescending(h => h.ChangeTime)
                .ThenByDescending(h => h.LogID)
                .Select(h => new
                {
                    LogID = h.LogID,
                    UserID = h.UserID,
                    UserName = h.User.UserName,
                    UserNickName = h.User.UserNickName,
                    ChangeType = h.ChangeType,
                    PointsChanged = h.PointsChanged,
                    ItemCode = h.ItemCode,
                    Description = h.Description,
                    ChangeTime = h.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .AsNoTracking()
                .ToListAsync();

            var fileName = $"wallet_history_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            return Json(histories, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private IQueryable<WalletHistory> BuildHistoryQuery(int? userId, string? type, string? q, DateTime? from, DateTime? to)
        {
            var query = _context.WalletHistories
                .Include(h => h.User)
                .AsNoTracking();

            if (userId.HasValue)
            {
                query = query.Where(h => h.UserID == userId.Value);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(h => h.ChangeType == type);
            }

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(h => (h.ItemCode != null && h.ItemCode.Contains(q)) || 
                                        h.Description.Contains(q));
            }

            if (from.HasValue)
            {
                query = query.Where(h => h.ChangeTime >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(h => h.ChangeTime <= to.Value.AddDays(1));
            }

            return query;
        }

        private string GenerateHistoryCsv(List<WalletHistory> histories)
        {
            var csv = new System.Text.StringBuilder();
            
            // CSV 標題行
            csv.AppendLine("LogID,UserID,UserName,UserNickName,ChangeType,PointsChanged,ItemCode,Description,ChangeTime");
            
            foreach (var h in histories)
            {
                csv.AppendLine($"{h.LogID},{h.UserID},\"{EscapeCsv(h.User?.UserName)}\",\"{EscapeCsv(h.User?.UserNickName)}\",\"{EscapeCsv(h.ChangeType)}\",{h.PointsChanged},\"{EscapeCsv(h.ItemCode)}\",\"{EscapeCsv(h.Description)}\",\"{h.ChangeTime:yyyy-MM-dd HH:mm:ss}\"");
            }
            
            return csv.ToString();
        }

        private string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\"", "\"\"");
        }

        #endregion

        #region EVoucher 管理

        /// <summary>
        /// 電子禮券列表頁面
        /// </summary>
        public async Task<IActionResult> EVouchers(int page = 1, int pageSize = 20, bool? isUsed = null, int? typeId = null)
        {
            var query = _context.EVouchers
                .Include(e => e.EVoucherType)
                .Include(e => e.User)
                .AsNoTracking();

            if (isUsed.HasValue)
            {
                query = query.Where(e => e.IsUsed == isUsed.Value);
            }

            if (typeId.HasValue)
            {
                query = query.Where(e => e.EVoucherTypeID == typeId.Value);
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var evouchers = await query
                .OrderByDescending(e => e.AcquiredTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            // 取得所有禮券類型供篩選使用
            var evoucherTypes = await _context.EVoucherTypes
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.IsUsed = isUsed;
            ViewBag.TypeId = typeId;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.EVoucherTypes = evoucherTypes;

            return View(evouchers);
        }

        /// <summary>
        /// 電子禮券明細頁面
        /// </summary>
        public async Task<IActionResult> EVoucherDetails(int id)
        {
            var evoucher = await _context.EVouchers
                .Include(e => e.EVoucherType)
                .Include(e => e.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EVoucherID == id);

            if (evoucher == null)
            {
                return NotFound("找不到指定的電子禮券");
            }

            // 取得相關的 Token 和核銷記錄
            var tokens = await _context.EVoucherTokens
                .Where(t => t.EVoucherID == id)
                .AsNoTracking()
                .ToListAsync();

            var redeemLogs = await _context.EVoucherRedeemLogs
                .Where(r => r.EVoucherID == id)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Tokens = tokens;
            ViewBag.RedeemLogs = redeemLogs;

            return View(evoucher);
        }

        #endregion

        #region Coupon 管理

        /// <summary>
        /// 優惠券列表頁面
        /// </summary>
        public async Task<IActionResult> Coupons(int page = 1, int pageSize = 20, bool? isUsed = null, int? typeId = null)
        {
            var query = _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .AsNoTracking();

            if (isUsed.HasValue)
            {
                query = query.Where(c => c.IsUsed == isUsed.Value);
            }

            if (typeId.HasValue)
            {
                query = query.Where(c => c.CouponTypeID == typeId.Value);
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var coupons = await query
                .OrderByDescending(c => c.AcquiredTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            // 取得所有優惠券類型供篩選使用
            var couponTypes = await _context.CouponTypes
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.IsUsed = isUsed;
            ViewBag.TypeId = typeId;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.CouponTypes = couponTypes;

            return View(coupons);
        }

        #endregion

        #region EVoucherToken 管理 (Tokens 頁面)

        /// <summary>
        /// EVoucherToken 清單頁面
        /// </summary>
        public async Task<IActionResult> Tokens(int page = 1, int pageSize = 20, string? search = null, bool? revoked = null)
        {
            var query = _context.EVoucherTokens
                .Include(t => t.EVoucher)
                .AsNoTracking();

            // 搜尋篩選
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Token.Contains(search) || 
                                        t.EVoucher.EVoucherCode.Contains(search));
            }

            // 撤銷狀態篩選
            if (revoked.HasValue)
            {
                query = query.Where(t => t.IsRevoked == revoked.Value);
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var tokens = await query
                .OrderByDescending(t => t.TokenID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.Search = search;
            ViewBag.Revoked = revoked;

            return View(tokens);
        }

        /// <summary>
        /// EVoucherToken CSV 匯出
        /// </summary>
        public async Task<IActionResult> TokensExportCsv(string? search = null, bool? revoked = null)
        {
            var query = BuildTokensQuery(search, revoked);
            
            var tokens = await query
                .OrderByDescending(t => t.TokenID)
                .AsNoTracking()
                .ToListAsync();

            var csv = GenerateTokensCsv(tokens);
            var fileName = $"evoucher_tokens_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        /// <summary>
        /// EVoucherToken JSON 匯出
        /// </summary>
        public async Task<IActionResult> TokensExportJson(string? search = null, bool? revoked = null)
        {
            var query = BuildTokensQuery(search, revoked);
            
            var tokens = await query
                .OrderByDescending(t => t.TokenID)
                .Select(t => new
                {
                    TokenID = t.TokenID,
                    Token = t.Token,
                    EVoucherID = t.EVoucherID,
                    EVoucherCode = t.EVoucher.EVoucherCode,
                    ExpiresAt = t.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsRevoked = t.IsRevoked
                })
                .AsNoTracking()
                .ToListAsync();

            return Json(tokens, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private IQueryable<EVoucherToken> BuildTokensQuery(string? search, bool? revoked)
        {
            var query = _context.EVoucherTokens
                .Include(t => t.EVoucher)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Token.Contains(search) || 
                                        t.EVoucher.EVoucherCode.Contains(search));
            }

            if (revoked.HasValue)
            {
                query = query.Where(t => t.IsRevoked == revoked.Value);
            }

            return query;
        }

        private string GenerateTokensCsv(List<EVoucherToken> tokens)
        {
            var csv = new System.Text.StringBuilder();
            
            // CSV 標題行
            csv.AppendLine("TokenID,Token,EVoucherID,EVoucherCode,ExpiresAt,IsRevoked");
            
            foreach (var t in tokens)
            {
                csv.AppendLine($"{t.TokenID},\"{EscapeCsv(t.Token)}\",{t.EVoucherID},\"{EscapeCsv(t.EVoucher?.EVoucherCode)}\",\"{t.ExpiresAt:yyyy-MM-dd HH:mm:ss}\",{t.IsRevoked}");
            }
            
            return csv.ToString();
        }

        #endregion

        #region EVoucherToken 管理 (原有功能)

        /// <summary>
        /// 電子禮券 Token 列表
        /// </summary>
        public async Task<IActionResult> EVoucherTokens(int page = 1, int pageSize = 20, int? evoucherId = null, bool? isRevoked = null)
        {
            var query = _context.EVoucherTokens
                .Include(t => t.EVoucher)
                .ThenInclude(e => e.User)
                .AsNoTracking();

            if (evoucherId.HasValue)
            {
                query = query.Where(t => t.EVoucherID == evoucherId.Value);
            }

            if (isRevoked.HasValue)
            {
                query = query.Where(t => t.IsRevoked == isRevoked.Value);
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var tokens = await query
                .OrderByDescending(t => t.ExpiresAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.EVoucherId = evoucherId;
            ViewBag.IsRevoked = isRevoked;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(tokens);
        }

        /// <summary>
        /// 電子禮券 Token 明細
        /// </summary>
        public async Task<IActionResult> EVoucherTokenDetails(int id)
        {
            var token = await _context.EVoucherTokens
                .Include(t => t.EVoucher)
                .ThenInclude(e => e.User)
                .Include(t => t.EVoucher.EVoucherType)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TokenID == id);

            if (token == null)
            {
                return NotFound("找不到指定的電子禮券 Token");
            }

            // 取得相關的兌換記錄
            var redeemLogs = await _context.EVoucherRedeemLogs
                .Where(r => r.TokenID == id)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.RedeemLogs = redeemLogs;
            return View(token);
        }

        #endregion

        #region EVoucherRedeemLog 管理

        /// <summary>
        /// EVoucher 兌換紀錄（簡化版）
        /// </summary>
        public async Task<IActionResult> RedeemLogs(int page = 1, int pageSize = 20, string? search = null, string? status = null, int? tokenId = null, int? userId = null, int? evoucherId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.EVoucherRedeemLogs
                .Include(r => r.EVoucherToken)
                .ThenInclude(t => t.EVoucher)
                .AsNoTracking();

            // 搜尋篩選
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.RedeemID.ToString().Contains(search) ||
                                        (r.EVoucherToken != null && r.EVoucherToken.Token.Contains(search)) ||
                                        (r.EVoucherToken != null && r.EVoucherToken.EVoucher.EVoucherCode.Contains(search)));
            }

            // 狀態篩選
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            // ID 篩選
            if (tokenId.HasValue)
            {
                query = query.Where(r => r.TokenID == tokenId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(r => r.UserID == userId.Value);
            }

            if (evoucherId.HasValue)
            {
                query = query.Where(r => r.EVoucherID == evoucherId.Value);
            }

            // 日期範圍篩選
            if (from.HasValue)
            {
                query = query.Where(r => r.ScannedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(r => r.ScannedAt <= to.Value.AddDays(1));
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var redeemLogs = await query
                .OrderByDescending(r => r.ScannedAt)
                .ThenByDescending(r => r.RedeemID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.TokenId = tokenId;
            ViewBag.UserId = userId;
            ViewBag.EVoucherId = evoucherId;
            ViewBag.From = from;
            ViewBag.To = to;

            return View(redeemLogs);
        }

        /// <summary>
        /// 兌換紀錄詳細
        /// </summary>
        public async Task<IActionResult> RedeemLogDetail(int id)
        {
            var redeemLog = await _context.EVoucherRedeemLogs
                .Include(r => r.EVoucherToken)
                .ThenInclude(t => t.EVoucher)
                .ThenInclude(e => e.EVoucherType)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RedeemID == id);

            if (redeemLog == null)
            {
                return NotFound("找不到指定的兌換記錄");
            }

            return View(redeemLog);
        }

        /// <summary>
        /// EVoucherRedeemLog CSV 匯出
        /// </summary>
        public async Task<IActionResult> RedeemLogsExportCsv(string? search = null, string? status = null, int? tokenId = null, int? userId = null, int? evoucherId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = BuildRedeemLogsQuery(search, status, tokenId, userId, evoucherId, from, to);
            
            var redeemLogs = await query
                .OrderByDescending(r => r.ScannedAt)
                .ThenByDescending(r => r.RedeemID)
                .AsNoTracking()
                .ToListAsync();

            var csv = GenerateRedeemLogsCsv(redeemLogs);
            var fileName = $"evoucher_redeemlogs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        /// <summary>
        /// EVoucherRedeemLog JSON 匯出
        /// </summary>
        public async Task<IActionResult> RedeemLogsExportJson(string? search = null, string? status = null, int? tokenId = null, int? userId = null, int? evoucherId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = BuildRedeemLogsQuery(search, status, tokenId, userId, evoucherId, from, to);
            
            var redeemLogs = await query
                .OrderByDescending(r => r.ScannedAt)
                .ThenByDescending(r => r.RedeemID)
                .Select(r => new
                {
                    RedeemID = r.RedeemID,
                    TokenID = r.TokenID,
                    Token = r.EVoucherToken != null ? r.EVoucherToken.Token : null,
                    EVoucherID = r.EVoucherID,
                    EVoucherCode = r.EVoucherToken != null && r.EVoucherToken.EVoucher != null ? r.EVoucherToken.EVoucher.EVoucherCode : null,
                    UserID = r.UserID,
                    ScannedAt = r.ScannedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = r.Status
                })
                .AsNoTracking()
                .ToListAsync();

            return Json(redeemLogs, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private IQueryable<EVoucherRedeemLog> BuildRedeemLogsQuery(string? search, string? status, int? tokenId, int? userId, int? evoucherId, DateTime? from, DateTime? to)
        {
            var query = _context.EVoucherRedeemLogs
                .Include(r => r.EVoucherToken)
                .ThenInclude(t => t.EVoucher)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.RedeemID.ToString().Contains(search) ||
                                        (r.EVoucherToken != null && r.EVoucherToken.Token.Contains(search)) ||
                                        (r.EVoucherToken != null && r.EVoucherToken.EVoucher.EVoucherCode.Contains(search)));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (tokenId.HasValue)
            {
                query = query.Where(r => r.TokenID == tokenId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(r => r.UserID == userId.Value);
            }

            if (evoucherId.HasValue)
            {
                query = query.Where(r => r.EVoucherID == evoucherId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(r => r.ScannedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(r => r.ScannedAt <= to.Value.AddDays(1));
            }

            return query;
        }

        private string GenerateRedeemLogsCsv(List<EVoucherRedeemLog> redeemLogs)
        {
            var csv = new System.Text.StringBuilder();
            
            // CSV 標題行
            csv.AppendLine("RedeemID,TokenID,Token,EVoucherID,EVoucherCode,UserID,ScannedAt,Status");
            
            foreach (var r in redeemLogs)
            {
                var token = r.EVoucherToken?.Token ?? "";
                var evoucherCode = r.EVoucherToken?.EVoucher?.EVoucherCode ?? "";
                csv.AppendLine($"{r.RedeemID},{r.TokenID},\"{EscapeCsv(token)}\",{r.EVoucherID},\"{EscapeCsv(evoucherCode)}\",{r.UserID},\"{r.ScannedAt:yyyy-MM-dd HH:mm:ss}\",\"{EscapeCsv(r.Status)}\"");
            }
            
            return csv.ToString();
        }

        /// <summary>
        /// 電子禮券兌換記錄列表（原有功能）
        /// </summary>
        public async Task<IActionResult> EVoucherRedeemLogs(int page = 1, int pageSize = 20, int? evoucherId = null, string? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.EVoucherRedeemLogs
                .Include(r => r.EVoucher)
                .ThenInclude(e => e.User)
                .AsNoTracking();

            if (evoucherId.HasValue)
            {
                query = query.Where(r => r.EVoucherID == evoucherId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.ScannedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.ScannedAt <= endDate.Value.AddDays(1));
            }

            var totalCount = await query.AsNoTracking().CountAsync();
            var redeemLogs = await query
                .OrderByDescending(r => r.ScannedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.EVoucherId = evoucherId;
            ViewBag.Status = status;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return View(redeemLogs);
        }

        /// <summary>
        /// 電子禮券兌換記錄明細
        /// </summary>
        public async Task<IActionResult> EVoucherRedeemLogDetails(int id)
        {
            var redeemLog = await _context.EVoucherRedeemLogs
                .Include(r => r.EVoucher)
                .ThenInclude(e => e.User)
                .Include(r => r.EVoucher.EVoucherType)
                .Include(r => r.EVoucherToken)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RedeemID == id);

            if (redeemLog == null)
            {
                return NotFound("找不到指定的兌換記錄");
            }

            return View(redeemLog);
        }

        #endregion

        #region 點數調整功能

        /// <summary>
        /// 顯示點數調整表單
        /// </summary>
        public async Task<IActionResult> Adjust(int userId)
        {
            var userWallet = await _context.UserWallets
                .Include(w => w.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserID == userId);

            if (userWallet == null)
            {
                return NotFound("找不到指定的會員錢包");
            }

            ViewBag.UserWallet = userWallet;
            return View();
        }

        /// <summary>
        /// 執行點數調整 - 交易性操作，含防重機制
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(int userId, int delta, string reason)
        {
            // 驗證輸入
            if (delta == 0)
            {
                TempData["Error"] = "調整數量不能為 0";
                return RedirectToAction(nameof(Adjust), new { userId });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "調整原因為必填";
                return RedirectToAction(nameof(Adjust), new { userId });
            }

            var adminUserId = User.Identity?.Name ?? "System"; // 獲取當前管理員 ID
            var traceId = HttpContext.TraceIdentifier;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 防重檢查：同一管理者 60 秒內若已對該 userId、同 delta、同 reason 送出，則不重複入帳
                var cutoffTime = DateTime.UtcNow.AddSeconds(-60);
                var duplicateCheck = await _context.WalletHistories
                    .AsNoTracking()
                    .AnyAsync(h => h.UserID == userId &&
                                 h.ChangeType == "ADMIN" &&
                                 h.PointsChanged == delta &&
                                 h.Description == reason &&
                                 h.ChangeTime >= cutoffTime);

                if (duplicateCheck)
                {
                    TempData["Error"] = "60 秒內已有相同的調整記錄，請勿重複操作";
                    return RedirectToAction(nameof(Adjust), new { userId });
                }

                // 查詢並更新錢包
                var userWallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserID == userId);

                if (userWallet == null)
                {
                    TempData["Error"] = "找不到指定的會員錢包";
                    return RedirectToAction(nameof(Adjust), new { userId });
                }

                // 檢查點數不能變為負數
                if (userWallet.UserPoint + delta < 0)
                {
                    TempData["Error"] = $"調整後點數不能為負數（當前：{userWallet.UserPoint}，調整：{delta}）";
                    return RedirectToAction(nameof(Adjust), new { userId });
                }

                var oldPoints = userWallet.UserPoint;
                userWallet.UserPoint += delta;

                // 新增錢包歷史記錄
                var walletHistory = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "ADMIN",
                    PointsChanged = delta,
                    ItemCode = "WalletAdjust",
                    Description = reason,
                    ChangeTime = DateTime.UtcNow
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 記錄 Serilog
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogInformation("管理員調整會員點數: UserId={UserId}, Delta={Delta}, OldPoints={OldPoints}, NewPoints={NewPoints}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    userId, delta, oldPoints, userWallet.UserPoint, reason, adminUserId, traceId);

                TempData["Success"] = $"成功調整點數 {(delta > 0 ? "+" : "")}{delta}，當前點數：{userWallet.UserPoint}";
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogError(ex, "點數調整失敗: UserId={UserId}, Delta={Delta}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    userId, delta, reason, adminUserId, traceId);

                TempData["Error"] = "點數調整失敗：" + ex.Message;
                return RedirectToAction(nameof(Adjust), new { userId });
            }
        }

        #endregion

        #region 會員資產管理

        /// <summary>
        /// 查看會員擁有的資產（優惠券和電子禮券）
        /// </summary>
        public async Task<IActionResult> MemberAssets(int userId, DateTime? from = null, DateTime? to = null)
        {
            // 載入會員資訊
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.User_ID == userId);

            if (user == null)
            {
                return NotFound("找不到指定的會員");
            }

            // 載入會員錢包
            var userWallet = await _context.UserWallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserID == userId);

            // 日期範圍篩選
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;
            var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

            // 查詢會員擁有的優惠券（未使用且有效）
            var ownedCoupons = await _context.Coupons
                .Include(c => c.CouponType)
                .Where(c => c.UserID == userId && 
                           c.AcquiredTime >= fromDate && 
                           c.AcquiredTime <= toDateEndOfDay)
                .AsNoTracking()
                .OrderByDescending(c => c.AcquiredTime)
                .ToListAsync();

            // 查詢會員擁有的電子禮券
            var ownedEVouchers = await _context.EVouchers
                .Include(e => e.EVoucherType)
                .Where(e => e.UserID == userId && 
                           e.AcquiredTime >= fromDate && 
                           e.AcquiredTime <= toDateEndOfDay)
                .AsNoTracking()
                .OrderByDescending(e => e.AcquiredTime)
                .ToListAsync();

            // 查詢會員的 EVoucherTokens
            var ownedTokens = await _context.EVoucherTokens
                .Include(t => t.EVoucher)
                .ThenInclude(e => e.EVoucherType)
                .Where(t => t.EVoucher.UserID == userId &&
                           t.ExpiresAt >= fromDate &&
                           t.ExpiresAt <= toDateEndOfDay)
                .AsNoTracking()
                .OrderByDescending(t => t.ExpiresAt)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.UserWallet = userWallet;
            ViewBag.OwnedCoupons = ownedCoupons;
            ViewBag.OwnedEVouchers = ownedEVouchers;
            ViewBag.OwnedTokens = ownedTokens;
            ViewBag.From = fromDate;
            ViewBag.To = toDate;

            // 統計資料
            ViewBag.CouponsUnusedCount = ownedCoupons.Count(c => !c.IsUsed);
            ViewBag.EVouchersUnusedCount = ownedEVouchers.Count(e => !e.IsUsed);
            ViewBag.TokensValidCount = ownedTokens.Count(t => !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

            return View();
        }

        #region Grant/Revoke 優惠券和電子禮券

        /// <summary>
        /// 發放優惠券給會員
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantCoupon(int userId, int couponTypeId, int count, string reason)
        {
            if (count <= 0 || count > 100)
            {
                TempData["Error"] = "發放數量必須在 1-100 之間";
                return RedirectToAction(nameof(MemberAssets), new { userId });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "發放原因為必填";
                return RedirectToAction(nameof(MemberAssets), new { userId });
            }

            var adminUserId = User.Identity?.Name ?? "System";
            var traceId = HttpContext.TraceIdentifier;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 檢查優惠券類型是否存在
                var couponType = await _context.CouponTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct => ct.CouponTypeID == couponTypeId);

                if (couponType == null)
                {
                    TempData["Error"] = "指定的優惠券類型不存在";
                    return RedirectToAction(nameof(MemberAssets), new { userId });
                }

                // 檢查會員是否存在
                var userExists = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.User_ID == userId);

                if (!userExists)
                {
                    TempData["Error"] = "指定的會員不存在";
                    return RedirectToAction(nameof(MemberAssets), new { userId });
                }

                // 批量建立優惠券
                var couponsToAdd = new List<Coupon>();
                for (int i = 0; i < count; i++)
                {
                    var couponCode = $"ADM{DateTime.UtcNow:yyyyMMdd}{userId:D6}{i + 1:D2}";
                    var coupon = new Coupon
                    {
                        CouponCode = couponCode,
                        CouponTypeID = couponTypeId,
                        UserID = userId,
                        IsUsed = false,
                        AcquiredTime = DateTime.UtcNow,
                        UsedTime = null,
                        UsedInOrderID = null
                    };
                    couponsToAdd.Add(coupon);
                }

                _context.Coupons.AddRange(couponsToAdd);

                // 記錄錢包歷史
                var walletHistory = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "ADMIN_GRANT",
                    PointsChanged = 0, // 優惠券發放不直接影響點數
                    ItemCode = "GrantCoupon",
                    Description = $"管理員發放優惠券 x{count} - {reason}",
                    ChangeTime = DateTime.UtcNow
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 記錄日誌
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogInformation("管理員發放優惠券: UserId={UserId}, CouponTypeId={CouponTypeId}, Count={Count}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    userId, couponTypeId, count, reason, adminUserId, traceId);

                TempData["Success"] = $"成功發放 {count} 張優惠券（{couponType.TypeName}）";
                return RedirectToAction(nameof(MemberAssets), new { userId });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogError(ex, "優惠券發放失敗: UserId={UserId}, CouponTypeId={CouponTypeId}, Count={Count}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    userId, couponTypeId, count, reason, adminUserId, traceId);

                TempData["Error"] = "優惠券發放失敗：" + ex.Message;
                return RedirectToAction(nameof(MemberAssets), new { userId });
            }
        }

        /// <summary>
        /// 撤銷優惠券
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeCoupon(int couponId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "撤銷原因為必填";
                return RedirectToAction(nameof(Index));
            }

            var adminUserId = User.Identity?.Name ?? "System";
            var traceId = HttpContext.TraceIdentifier;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var coupon = await _context.Coupons
                    .Include(c => c.CouponType)
                    .FirstOrDefaultAsync(c => c.CouponID == couponId);

                if (coupon == null)
                {
                    TempData["Error"] = "找不到指定的優惠券";
                    return RedirectToAction(nameof(Index));
                }

                if (coupon.IsUsed)
                {
                    TempData["Error"] = "已使用的優惠券無法撤銷";
                    return RedirectToAction(nameof(MemberAssets), new { userId = coupon.UserID });
                }

                // 刪除優惠券
                _context.Coupons.Remove(coupon);

                // 記錄錢包歷史
                var walletHistory = new WalletHistory
                {
                    UserID = coupon.UserID,
                    ChangeType = "ADMIN_REVOKE",
                    PointsChanged = 0, // 優惠券撤銷不直接影響點數
                    ItemCode = "RevokeCoupon",
                    Description = $"管理員撤銷優惠券 {coupon.CouponCode} - {reason}",
                    ChangeTime = DateTime.UtcNow
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 記錄日誌
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogInformation("管理員撤銷優惠券: CouponId={CouponId}, CouponCode={CouponCode}, UserId={UserId}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    couponId, coupon.CouponCode, coupon.UserID, reason, adminUserId, traceId);

                TempData["Success"] = $"成功撤銷優惠券 {coupon.CouponCode}";
                return RedirectToAction(nameof(MemberAssets), new { userId = coupon.UserID });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogError(ex, "優惠券撤銷失敗: CouponId={CouponId}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    couponId, reason, adminUserId, traceId);

                TempData["Error"] = "優惠券撤銷失敗：" + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 發放電子禮券給會員
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantEVoucher(int userId, int eVoucherTypeId, int count, string reason)
        {
            if (count <= 0 || count > 100)
            {
                TempData["Error"] = "發放數量必須在 1-100 之間";
                return RedirectToAction(nameof(MemberAssets), new { userId });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "發放原因為必填";
                return RedirectToAction(nameof(MemberAssets), new { userId });
            }

            var adminUserId = User.Identity?.Name ?? "System";
            var traceId = HttpContext.TraceIdentifier;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 檢查電子禮券類型是否存在
                var eVoucherType = await _context.EVoucherTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(et => et.EVoucherTypeID == eVoucherTypeId);

                if (eVoucherType == null)
                {
                    TempData["Error"] = "指定的電子禮券類型不存在";
                    return RedirectToAction(nameof(MemberAssets), new { userId });
                }

                // 批量建立電子禮券和對應的 Token
                for (int i = 0; i < count; i++)
                {
                    var eVoucherCode = $"EVA{DateTime.UtcNow:yyyyMMdd}{userId:D6}{i + 1:D2}";
                    var eVoucher = new EVoucher
                    {
                        EVoucherCode = eVoucherCode,
                        EVoucherTypeID = eVoucherTypeId,
                        UserID = userId,
                        IsUsed = false,
                        AcquiredTime = DateTime.UtcNow,
                        UsedTime = null
                    };

                    _context.EVouchers.Add(eVoucher);
                    await _context.SaveChangesAsync(); // 需要先儲存以取得 EVoucherID

                    // 建立對應的 Token
                    var tokenCode = $"TKN{DateTime.UtcNow:yyyyMMdd}{eVoucher.EVoucherID:D6}";
                    var token = new EVoucherToken
                    {
                        Token = tokenCode,
                        EVoucherID = eVoucher.EVoucherID,
                        ExpiresAt = DateTime.UtcNow.AddDays(30), // 30天後到期
                        IsRevoked = false
                    };

                    _context.EVoucherTokens.Add(token);
                }

                // 記錄錢包歷史
                var walletHistory = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "ADMIN_GRANT",
                    PointsChanged = 0, // 電子禮券發放不直接影響點數
                    ItemCode = "GrantEVoucher",
                    Description = $"管理員發放電子禮券 x{count} - {reason}",
                    ChangeTime = DateTime.UtcNow
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 記錄日誌
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogInformation("管理員發放電子禮券: UserId={UserId}, EVoucherTypeId={EVoucherTypeId}, Count={Count}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    userId, eVoucherTypeId, count, reason, adminUserId, traceId);

                TempData["Success"] = $"成功發放 {count} 張電子禮券（{eVoucherType.TypeName}）";
                return RedirectToAction(nameof(MemberAssets), new { userId });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogError(ex, "電子禮券發放失敗: UserId={UserId}, EVoucherTypeId={EVoucherTypeId}, Count={Count}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    userId, eVoucherTypeId, count, reason, adminUserId, traceId);

                TempData["Error"] = "電子禮券發放失敗：" + ex.Message;
                return RedirectToAction(nameof(MemberAssets), new { userId });
            }
        }

        /// <summary>
        /// 撤銷電子禮券
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeEVoucher(int eVoucherId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "撤銷原因為必填";
                return RedirectToAction(nameof(Index));
            }

            var adminUserId = User.Identity?.Name ?? "System";
            var traceId = HttpContext.TraceIdentifier;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var eVoucher = await _context.EVouchers
                    .Include(e => e.EVoucherType)
                    .FirstOrDefaultAsync(e => e.EVoucherID == eVoucherId);

                if (eVoucher == null)
                {
                    TempData["Error"] = "找不到指定的電子禮券";
                    return RedirectToAction(nameof(Index));
                }

                if (eVoucher.IsUsed)
                {
                    TempData["Error"] = "已使用的電子禮券無法撤銷";
                    return RedirectToAction(nameof(MemberAssets), new { userId = eVoucher.UserID });
                }

                // 先撤銷相關的 Token
                var relatedTokens = await _context.EVoucherTokens
                    .Where(t => t.EVoucherID == eVoucherId)
                    .ToListAsync();

                foreach (var token in relatedTokens)
                {
                    token.IsRevoked = true;
                }

                // 刪除電子禮券
                _context.EVouchers.Remove(eVoucher);

                // 記錄錢包歷史
                var walletHistory = new WalletHistory
                {
                    UserID = eVoucher.UserID,
                    ChangeType = "ADMIN_REVOKE",
                    PointsChanged = 0, // 電子禮券撤銷不直接影響點數
                    ItemCode = "RevokeEVoucher",
                    Description = $"管理員撤銷電子禮券 {eVoucher.EVoucherCode} - {reason}",
                    ChangeTime = DateTime.UtcNow
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 記錄日誌
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogInformation("管理員撤銷電子禮券: EVoucherId={EVoucherId}, EVoucherCode={EVoucherCode}, UserId={UserId}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    eVoucherId, eVoucher.EVoucherCode, eVoucher.UserID, reason, adminUserId, traceId);

                TempData["Success"] = $"成功撤銷電子禮券 {eVoucher.EVoucherCode}";
                return RedirectToAction(nameof(MemberAssets), new { userId = eVoucher.UserID });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<AdminWalletController>>();
                logger.LogError(ex, "電子禮券撤銷失敗: EVoucherId={EVoucherId}, Reason={Reason}, AdminId={AdminId}, TraceId={TraceId}",
                    eVoucherId, reason, adminUserId, traceId);

                TempData["Error"] = "電子禮券撤銷失敗：" + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion

        #region 總收支明細（CombinedLedger）

        /// <summary>
        /// 統一的收支明細頁面 - 整合點數、優惠券、電子禮券的所有異動
        /// </summary>
        public async Task<IActionResult> CombinedLedger(int page = 1, int pageSize = 20, 
            string? type = null, DateTime? from = null, DateTime? to = null, int? userId = null)
        {
            // 日期範圍預設最近30天
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;
            var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

            // 查詢錢包歷史記錄
            var walletQuery = _context.WalletHistories
                .Include(h => h.User)
                .Where(h => h.ChangeTime >= fromDate && h.ChangeTime <= toDateEndOfDay)
                .AsNoTracking();

            // 查詢優惠券異動
            var couponQuery = _context.Coupons
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .Where(c => c.AcquiredTime >= fromDate && c.AcquiredTime <= toDateEndOfDay)
                .AsNoTracking();

            // 查詢電子禮券異動
            var evoucherQuery = _context.EVouchers
                .Include(e => e.User)
                .Include(e => e.EVoucherType)
                .Where(e => e.AcquiredTime >= fromDate && e.AcquiredTime <= toDateEndOfDay)
                .AsNoTracking();

            // 用戶篩選
            if (userId.HasValue)
            {
                walletQuery = walletQuery.Where(h => h.UserID == userId.Value);
                couponQuery = couponQuery.Where(c => c.UserID == userId.Value);
                evoucherQuery = evoucherQuery.Where(e => e.UserID == userId.Value);
            }

            // 建立統一的異動記錄列表
            var combinedRecords = new List<object>();

            // 錢包記錄
            var walletRecords = await walletQuery.ToListAsync();
            foreach (var record in walletRecords)
            {
                combinedRecords.Add(new
                {
                    Type = "Points",
                    TypeDisplay = "點數異動",
                    DateTime = record.ChangeTime,
                    UserID = record.UserID,
                    UserName = record.User?.UserNickName ?? record.User?.UserName ?? "未知",
                    Description = record.Description,
                    Amount = record.PointsChanged,
                    ItemCode = record.ItemCode,
                    ChangeType = record.ChangeType,
                    IsInflow = record.PointsChanged > 0,
                    BadgeClass = record.PointsChanged > 0 ? "bg-success" : "bg-danger",
                    IconClass = "fas fa-coins"
                });
            }

            // 優惠券記錄
            var couponRecords = await couponQuery.ToListAsync();
            foreach (var record in couponRecords)
            {
                combinedRecords.Add(new
                {
                    Type = "Coupon",
                    TypeDisplay = "優惠券",
                    DateTime = record.AcquiredTime,
                    UserID = record.UserID,
                    UserName = record.User?.UserNickName ?? record.User?.UserName ?? "未知",
                    Description = $"獲得優惠券：{record.CouponType?.TypeName ?? "未知類型"}",
                    Amount = 0, // 優惠券不直接影響點數
                    ItemCode = record.CouponCode,
                    ChangeType = "COUPON_ACQUIRED",
                    IsInflow = true,
                    BadgeClass = "bg-info",
                    IconClass = "fas fa-ticket-alt"
                });
            }

            // 電子禮券記錄
            var evoucherRecords = await evoucherQuery.ToListAsync();
            foreach (var record in evoucherRecords)
            {
                combinedRecords.Add(new
                {
                    Type = "EVoucher",
                    TypeDisplay = "電子禮券",
                    DateTime = record.AcquiredTime,
                    UserID = record.UserID,
                    UserName = record.User?.UserNickName ?? record.User?.UserName ?? "未知",
                    Description = $"獲得電子禮券：{record.EVoucherType?.TypeName ?? "未知類型"}",
                    Amount = 0, // 電子禮券不直接影響點數
                    ItemCode = record.EVoucherCode,
                    ChangeType = "EVOUCHER_ACQUIRED",
                    IsInflow = true,
                    BadgeClass = "bg-warning text-dark",
                    IconClass = "fas fa-gift"
                });
            }

            // 按時間排序並分頁
            var sortedRecords = combinedRecords
                .OrderByDescending(r => r.DateTime)
                .ToList();

            var totalCount = sortedRecords.Count;
            var pagedRecords = sortedRecords
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 統計資料
            var totalInflow = combinedRecords.Where(r => r.IsInflow && r.Type == "Points").Sum(r => r.Amount);
            var totalOutflow = combinedRecords.Where(r => !r.IsInflow && r.Type == "Points").Sum(r => Math.Abs(r.Amount));
            var couponCount = combinedRecords.Count(r => r.Type == "Coupon");
            var evoucherCount = combinedRecords.Count(r => r.Type == "EVoucher");

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.Type = type;
            ViewBag.From = fromDate;
            ViewBag.To = toDate;
            ViewBag.UserId = userId;
            ViewBag.Records = pagedRecords;

            ViewBag.TotalInflow = totalInflow;
            ViewBag.TotalOutflow = totalOutflow;
            ViewBag.CouponCount = couponCount;
            ViewBag.EVoucherCount = evoucherCount;

            return View();
        }

        /// <summary>
        /// 總收支明細 CSV 匯出
        /// </summary>
        public async Task<IActionResult> CombinedLedgerExportCsv(string? type = null, DateTime? from = null, DateTime? to = null, int? userId = null)
        {
            // 重用查詢邏輯
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;
            var toDateEndOfDay = toDate.Date.AddDays(1).AddTicks(-1);

            var combinedRecords = new List<object>();

            // 查詢各類記錄（簡化版，僅用於匯出）
            var walletRecords = await _context.WalletHistories
                .Include(h => h.User)
                .Where(h => h.ChangeTime >= fromDate && h.ChangeTime <= toDateEndOfDay)
                .Where(h => !userId.HasValue || h.UserID == userId.Value)
                .AsNoTracking()
                .ToListAsync();

            foreach (var record in walletRecords)
            {
                combinedRecords.Add(new
                {
                    類型 = "點數異動",
                    時間 = record.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    會員ID = record.UserID,
                    會員名稱 = record.User?.UserNickName ?? record.User?.UserName ?? "未知",
                    描述 = record.Description,
                    點數變動 = record.PointsChanged,
                    項目代碼 = record.ItemCode,
                    異動類型 = record.ChangeType
                });
            }

            using var exportService = new GameSpace.Areas.MiniGame.Services.ExportService();
            return ExportService.CreateJsonFile(
                ExportService.CreateExportData(combinedRecords, new { 
                    查詢範圍 = $"{fromDate:yyyy-MM-dd} ~ {toDate:yyyy-MM-dd}",
                    篩選條件 = new { 類型 = type ?? "全部", 會員ID = userId?.ToString() ?? "全部" }
                }), 
                "combined_ledger"
            );
        }

        /// <summary>
        /// 總收支明細 JSON 匯出
        /// </summary>
        public async Task<IActionResult> CombinedLedgerExportJson(string? type = null, DateTime? from = null, DateTime? to = null, int? userId = null)
        {
            // 重用 CSV 邏輯但返回 JSON
            return await CombinedLedgerExportCsv(type, from, to, userId);
        }

        #endregion

        #endregion

        #region Coupons/EVouchers 匯出功能

        /// <summary>
        /// 優惠券 CSV 匯出
        /// </summary>
        public async Task<IActionResult> CouponsExportCsv(bool? isUsed = null, int? typeId = null, 
            int? userId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Coupons
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .AsNoTracking();

            // 套用篩選條件（重用現有邏輯）
            if (isUsed.HasValue)
            {
                query = query.Where(c => c.IsUsed == isUsed.Value);
            }

            if (typeId.HasValue)
            {
                query = query.Where(c => c.CouponTypeID == typeId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(c => c.UserID == userId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(c => c.AcquiredTime >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(c => c.AcquiredTime <= to.Value.AddDays(1).AddTicks(-1));
            }

            var coupons = await query
                .OrderByDescending(c => c.AcquiredTime)
                .AsNoTracking()
                .ToListAsync();

            // 生成 CSV 內容
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,優惠券代碼,類型,狀態,發放時間,使用時間,擁有者");

            foreach (var coupon in coupons)
            {
                csv.AppendLine($"{coupon.CouponID}," +
                             $"\"{coupon.CouponCode}\"," +
                             $"\"{coupon.CouponType?.TypeName ?? "未知"}\"," +
                             $"\"{(coupon.IsUsed ? "已使用" : "未使用")}\"," +
                             $"\"{coupon.AcquiredTime:yyyy-MM-dd HH:mm:ss}\"," +
                             $"\"{(coupon.UsedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "")}\"," +
                             $"\"{coupon.User?.UserNickName ?? coupon.User?.UserName ?? "未知"}\"");
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"coupons_{timestamp}.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), 
                       "text/csv; charset=utf-8", fileName);
        }

        /// <summary>
        /// 優惠券 JSON 匯出
        /// </summary>
        public async Task<IActionResult> CouponsExportJson(bool? isUsed = null, int? typeId = null, 
            int? userId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.Coupons
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .AsNoTracking();

            // 套用篩選條件（重用現有邏輯）
            if (isUsed.HasValue)
            {
                query = query.Where(c => c.IsUsed == isUsed.Value);
            }

            if (typeId.HasValue)
            {
                query = query.Where(c => c.CouponTypeID == typeId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(c => c.UserID == userId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(c => c.AcquiredTime >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(c => c.AcquiredTime <= to.Value.AddDays(1).AddTicks(-1));
            }

            var coupons = await query
                .OrderByDescending(c => c.AcquiredTime)
                .Select(c => new
                {
                    ID = c.CouponID,
                    優惠券代碼 = c.CouponCode,
                    類型 = c.CouponType.TypeName,
                    狀態 = c.IsUsed ? "已使用" : "未使用",
                    發放時間 = c.AcquiredTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    使用時間 = c.UsedTime.HasValue ? c.UsedTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    擁有者 = c.User.UserNickName ?? c.User.UserName,
                    會員ID = c.UserID
                })
                .AsNoTracking()
                .ToListAsync();

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"coupons_{timestamp}.json";

            return File(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(coupons, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            })), "application/json; charset=utf-8", fileName);
        }

        /// <summary>
        /// 電子禮券 CSV 匯出
        /// </summary>
        public async Task<IActionResult> EVouchersExportCsv(bool? isUsed = null, int? typeId = null, 
            int? userId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.EVouchers
                .Include(e => e.User)
                .Include(e => e.EVoucherType)
                .AsNoTracking();

            // 套用篩選條件（重用現有邏輯）
            if (isUsed.HasValue)
            {
                query = query.Where(e => e.IsUsed == isUsed.Value);
            }

            if (typeId.HasValue)
            {
                query = query.Where(e => e.EVoucherTypeID == typeId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(e => e.UserID == userId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.AcquiredTime >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.AcquiredTime <= to.Value.AddDays(1).AddTicks(-1));
            }

            var evouchers = await query
                .OrderByDescending(e => e.AcquiredTime)
                .AsNoTracking()
                .ToListAsync();

            // 生成 CSV 內容
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,禮券代碼,類型,狀態,發放時間,使用時間,擁有者");

            foreach (var evoucher in evouchers)
            {
                csv.AppendLine($"{evoucher.EVoucherID}," +
                             $"\"{evoucher.EVoucherCode}\"," +
                             $"\"{evoucher.EVoucherType?.TypeName ?? "未知"}\"," +
                             $"\"{(evoucher.IsUsed ? "已使用" : "未使用")}\"," +
                             $"\"{evoucher.AcquiredTime:yyyy-MM-dd HH:mm:ss}\"," +
                             $"\"{(evoucher.UsedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "")}\"," +
                             $"\"{evoucher.User?.UserNickName ?? evoucher.User?.UserName ?? "未知"}\"");
            }

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"evouchers_{timestamp}.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), 
                       "text/csv; charset=utf-8", fileName);
        }

        /// <summary>
        /// 電子禮券 JSON 匯出
        /// </summary>
        public async Task<IActionResult> EVouchersExportJson(bool? isUsed = null, int? typeId = null, 
            int? userId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.EVouchers
                .Include(e => e.User)
                .Include(e => e.EVoucherType)
                .AsNoTracking();

            // 套用篩選條件（重用現有邏輯）
            if (isUsed.HasValue)
            {
                query = query.Where(e => e.IsUsed == isUsed.Value);
            }

            if (typeId.HasValue)
            {
                query = query.Where(e => e.EVoucherTypeID == typeId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(e => e.UserID == userId.Value);
            }

            if (from.HasValue)
            {
                query = query.Where(e => e.AcquiredTime >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.AcquiredTime <= to.Value.AddDays(1).AddTicks(-1));
            }

            var evouchers = await query
                .OrderByDescending(e => e.AcquiredTime)
                .Select(e => new
                {
                    ID = e.EVoucherID,
                    禮券代碼 = e.EVoucherCode,
                    類型 = e.EVoucherType.TypeName,
                    狀態 = e.IsUsed ? "已使用" : "未使用",
                    發放時間 = e.AcquiredTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    使用時間 = e.UsedTime.HasValue ? e.UsedTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : null,
                    擁有者 = e.User.UserNickName ?? e.User.UserName,
                    會員ID = e.UserID
                })
                .AsNoTracking()
                .ToListAsync();

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"evouchers_{timestamp}.json";

            return File(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(evouchers, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            })), "application/json; charset=utf-8", fileName);
        }

        #endregion
    }
}