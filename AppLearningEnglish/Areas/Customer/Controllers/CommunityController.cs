using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CommunityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommunityController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.ForumPosts
                .AsNoTracking()
                .Include(x => x.ApplicationUser)
                .Include(x => x.Comments)
                .OrderByDescending(x => x.CreatedAt)
                .Take(40)
                .ToListAsync();

            return View(posts);
        }

        public IActionResult Create() => View(new ForumPost());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumPost model)
        {
            if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Body))
            {
                ModelState.AddModelError(string.Empty, "Nhập tiêu đề và nội dung.");
                return View(model);
            }

            model.ApplicationUserId = GetUserId();
            model.Title = model.Title.Trim();
            model.Body = model.Body.Trim();
            model.CreatedAt = DateTime.UtcNow;
            _context.ForumPosts.Add(model);
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã đăng bài.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.ForumPosts
                .Include(x => x.ApplicationUser)
                .Include(x => x.Comments)
                .ThenInclude(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            ViewBag.IsFollowing = await _context.ForumFollows.AnyAsync(x =>
                x.FollowerId == GetUserId() && x.FollowedUserId == post.ApplicationUserId);

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comment(int id, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                TempData["error"] = "Nhập nội dung bình luận.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.ForumComments.Add(new ForumComment
            {
                ForumPostId = id,
                ApplicationUserId = GetUserId(),
                Body = body.Trim(),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.LikeCount += 1;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(string userId)
        {
            var me = GetUserId();
            if (string.IsNullOrWhiteSpace(userId) || userId == me)
            {
                return RedirectToAction(nameof(Index));
            }

            var existing = await _context.ForumFollows.FirstOrDefaultAsync(x =>
                x.FollowerId == me && x.FollowedUserId == userId);

            if (existing == null)
            {
                _context.ForumFollows.Add(new ForumFollow
                {
                    FollowerId = me,
                    FollowedUserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
                TempData["success"] = "Đã theo dõi.";
            }
            else
            {
                _context.ForumFollows.Remove(existing);
                TempData["success"] = "Đã bỏ theo dõi.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private string GetUserId() =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
    }
}
