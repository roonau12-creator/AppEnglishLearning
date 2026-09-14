using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GrammarTopicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GrammarTopicController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.GrammarTopics
                .AsNoTracking()
                .Include(x => x.Items)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                return View(new GrammarTopic { Level = "Beginner", SortOrder = 1 });
            }

            var item = await _context.GrammarTopics.FindAsync(id.Value);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(GrammarTopic model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == 0)
            {
                _context.GrammarTopics.Add(model);
                TempData["success"] = "Đã tạo chủ điểm.";
            }
            else
            {
                var existing = await _context.GrammarTopics.FindAsync(model.Id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Title = model.Title;
                existing.Level = model.Level;
                existing.SortOrder = model.SortOrder;
                existing.Explanation = model.Explanation;
                existing.Examples = model.Examples;
                TempData["success"] = "Đã cập nhật chủ điểm.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.GrammarTopics
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(GrammarItem item)
        {
            if (string.IsNullOrWhiteSpace(item.QuestionText) || string.IsNullOrWhiteSpace(item.Answer))
            {
                TempData["error"] = "Cần câu hỏi và đáp án.";
                return RedirectToAction(nameof(Details), new { id = item.GrammarTopicId });
            }

            _context.GrammarItems.Add(item);
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã thêm câu luyện.";
            return RedirectToAction(nameof(Details), new { id = item.GrammarTopicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.GrammarItems.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy câu." });
            }

            _context.GrammarItems.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa câu." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.GrammarTopics.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy chủ điểm." });
            }

            _context.GrammarTopics.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa chủ điểm." });
        }
    }
}
