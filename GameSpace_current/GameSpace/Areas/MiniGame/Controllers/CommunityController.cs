using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class CommunityController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public CommunityController(GameSpaceDbContext context)
        {
            _context = context;
        }

        // 社群首頁
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            
            // 獲取用戶的寵物
            var userPet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // 獲取最近的聊天訊息
            var recentMessages = await _context.ChatMessages
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Take(20)
                .ToListAsync();

            // 獲取在線用戶數（簡化：假設最近5分鐘內有活動的用戶為在線）
            var onlineUsers = await _context.ChatMessages
                .Where(c => c.CreatedAt >= DateTime.UtcNow.AddMinutes(-5))
                .Select(c => c.UserId)
                .Distinct()
                .CountAsync();

            ViewBag.UserPet = userPet;
            ViewBag.RecentMessages = recentMessages;
            ViewBag.OnlineUsers = onlineUsers;

            return View();
        }

        // 聊天室
        public async Task<IActionResult> Chat()
        {
            var userId = GetCurrentUserId();
            var messages = await _context.ChatMessages
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Take(50)
                .ToListAsync();

            var onlineUsers = await _context.ChatMessages
                .Where(c => c.CreatedAt >= DateTime.UtcNow.AddMinutes(-5))
                .Select(c => c.UserId)
                .Distinct()
                .CountAsync();

            ViewBag.Messages = messages;
            ViewBag.OnlineUsers = onlineUsers;

            return View();
        }

        // 發送聊天訊息
        [HttpPost]
        public async Task<IActionResult> SendMessage(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "訊息內容不能為空" });
            }

            var userId = GetCurrentUserId();
            var message = new ChatMessage
            {
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            // 返回新訊息的HTML
            var user = await _context.Users.FindAsync(userId);
            var messageHtml = $@"
                <div class='message-item'>
                    <div class='message-header'>
                        <strong>{user?.Username}</strong>
                        <small class='text-muted'>{message.CreatedAt:HH:mm}</small>
                    </div>
                    <div class='message-content'>{message.Content}</div>
                </div>";

            return Json(new { success = true, messageHtml = messageHtml });
        }

        // 獲取新訊息
        [HttpGet]
        public async Task<IActionResult> GetNewMessages(DateTime lastMessageTime)
        {
            var messages = await _context.ChatMessages
                .Include(c => c.User)
                .Where(c => c.CreatedAt > lastMessageTime)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var messageHtmls = messages.Select(m => $@"
                <div class='message-item'>
                    <div class='message-header'>
                        <strong>{m.User?.Username}</strong>
                        <small class='text-muted'>{m.CreatedAt:HH:mm}</small>
                    </div>
                    <div class='message-content'>{m.Content}</div>
                </div>").ToList();

            return Json(new { messages = messageHtmls, lastMessageTime = messages.LastOrDefault()?.CreatedAt });
        }

        // 好友列表
        public async Task<IActionResult> Friends()
        {
            var userId = GetCurrentUserId();
            var friendships = await _context.Friendships
                .Include(f => f.Friend)
                .Where(f => f.UserId == userId && f.Status == "Accepted")
                .ToListAsync();

            return View(friendships);
        }

        // 添加好友
        [HttpPost]
        public async Task<IActionResult> AddFriend(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Json(new { success = false, message = "請輸入用戶名" });
            }

            var userId = GetCurrentUserId();
            var friend = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (friend == null)
            {
                return Json(new { success = false, message = "找不到該用戶" });
            }

            if (friend.UserId == userId)
            {
                return Json(new { success = false, message = "不能添加自己為好友" });
            }

            // 檢查是否已經是好友
            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendId == friend.UserId);

            if (existingFriendship != null)
            {
                return Json(new { success = false, message = "已經是好友關係" });
            }

            var friendship = new Friendship
            {
                UserId = userId,
                FriendId = friend.UserId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "好友請求已發送" });
        }

        // 接受好友請求
        [HttpPost]
        public async Task<IActionResult> AcceptFriend(int friendshipId)
        {
            var userId = GetCurrentUserId();
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.FriendshipId == friendshipId && f.FriendId == userId && f.Status == "Pending");

            if (friendship == null)
            {
                return Json(new { success = false, message = "找不到好友請求" });
            }

            friendship.Status = "Accepted";
            friendship.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "好友請求已接受" });
        }

        // 拒絕好友請求
        [HttpPost]
        public async Task<IActionResult> RejectFriend(int friendshipId)
        {
            var userId = GetCurrentUserId();
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.FriendshipId == friendshipId && f.FriendId == userId && f.Status == "Pending");

            if (friendship == null)
            {
                return Json(new { success = false, message = "找不到好友請求" });
            }

            friendship.Status = "Rejected";
            friendship.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "好友請求已拒絕" });
        }

        // 好友請求列表
        public async Task<IActionResult> FriendRequests()
        {
            var userId = GetCurrentUserId();
            var requests = await _context.Friendships
                .Include(f => f.User)
                .Where(f => f.FriendId == userId && f.Status == "Pending")
                .ToListAsync();

            return View(requests);
        }

        // 排行榜
        public async Task<IActionResult> Leaderboard()
        {
            // 寵物等級排行榜
            var petLeaderboard = await _context.Pets
                .Include(p => p.User)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.Experience)
                .Take(20)
                .ToListAsync();

            // 用戶點數排行榜
            var pointLeaderboard = await _context.UserWallets
                .Include(uw => uw.User)
                .OrderByDescending(uw => uw.UserPoint)
                .Take(20)
                .ToListAsync();

            ViewBag.PetLeaderboard = petLeaderboard;
            ViewBag.PointLeaderboard = pointLeaderboard;

            return View();
        }

        // 用戶資料
        public async Task<IActionResult> Profile(int? id)
        {
            var userId = id ?? GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var userWallet = await _context.UserWallets
                .FirstOrDefaultAsync(uw => uw.UserId == userId);

            var userIntro = await _context.UserIntroductions
                .FirstOrDefaultAsync(ui => ui.UserId == userId);

            ViewBag.Pet = pet;
            ViewBag.UserWallet = userWallet;
            ViewBag.UserIntro = userIntro;
            ViewBag.IsOwnProfile = userId == GetCurrentUserId();

            return View(user);
        }

        private int GetCurrentUserId()
        {
            // 暫時返回固定用戶ID，實際應該從認證中獲取
            return 1;
        }
    }
}