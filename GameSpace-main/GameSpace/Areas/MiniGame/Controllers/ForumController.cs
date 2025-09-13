using Microsoft.AspNetCore.Mvc;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class ForumController : Controller
    {
        private readonly GameSpaceDbContext _context;

        public ForumController(GameSpaceDbContext context)
        {
            _context = context;
        }

        // 論壇首頁
        public async Task<IActionResult> Index()
        {
            var forums = await _context.Forums
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(forums);
        }

        // 創建新討論
        public IActionResult Create()
        {
            return View();
        }

        // 創建新討論 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Forum forum)
        {
            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();
                forum.UserId = userId;
                forum.CreatedAt = DateTime.UtcNow;
                forum.UpdatedAt = DateTime.UtcNow;
                forum.IsActive = true;

                _context.Forums.Add(forum);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = forum.ForumId });
            }

            return View(forum);
        }

        // 討論詳情
        public async Task<IActionResult> Details(int id)
        {
            var forum = await _context.Forums
                .Include(f => f.User)
                .Include(f => f.ForumComments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(f => f.ForumId == id && f.IsActive);

            if (forum == null)
            {
                return NotFound();
            }

            // 增加瀏覽次數
            forum.ViewCount++;
            await _context.SaveChangesAsync();

            return View(forum);
        }

        // 編輯討論
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var forum = await _context.Forums
                .FirstOrDefaultAsync(f => f.ForumId == id && f.UserId == userId && f.IsActive);

            if (forum == null)
            {
                return NotFound();
            }

            return View(forum);
        }

        // 編輯討論 (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Forum forum)
        {
            if (id != forum.ForumId)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();
            var existingForum = await _context.Forums
                .FirstOrDefaultAsync(f => f.ForumId == id && f.UserId == userId && f.IsActive);

            if (existingForum == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingForum.Title = forum.Title;
                existingForum.Content = forum.Content;
                existingForum.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = forum.ForumId });
            }

            return View(forum);
        }

        // 刪除討論
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var forum = await _context.Forums
                .FirstOrDefaultAsync(f => f.ForumId == id && f.UserId == userId && f.IsActive);

            if (forum == null)
            {
                return NotFound();
            }

            return View(forum);
        }

        // 刪除討論 (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            var forum = await _context.Forums
                .FirstOrDefaultAsync(f => f.ForumId == id && f.UserId == userId && f.IsActive);

            if (forum != null)
            {
                forum.IsActive = false;
                forum.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // 添加評論
        [HttpPost]
        public async Task<IActionResult> AddComment(int forumId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "評論內容不能為空" });
            }

            var userId = GetCurrentUserId();
            var comment = new ForumComment
            {
                ForumId = forumId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "評論添加成功" });
        }

        // 我的討論
        public async Task<IActionResult> MyForums()
        {
            var userId = GetCurrentUserId();
            var forums = await _context.Forums
                .Where(f => f.UserId == userId && f.IsActive)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(forums);
        }

        // 搜尋討論
        public async Task<IActionResult> Search(string keyword)
        {
            var forums = await _context.Forums
                .Include(f => f.User)
                .Where(f => f.IsActive && 
                           (f.Title.Contains(keyword) || f.Content.Contains(keyword)))
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            return View("Index", forums);
        }

        private int GetCurrentUserId()
        {
            // 暫時返回固定用戶ID，實際應該從認證中獲取
            return 1;
        }
    }
}