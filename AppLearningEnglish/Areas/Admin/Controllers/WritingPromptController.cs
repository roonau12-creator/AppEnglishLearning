using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class WritingPromptController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WritingPromptController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.WritingPrompts
                .AsNoTracking()
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Title)
                .ToListAsync();
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                return View(new WritingPrompt { Level = "Beginner", MinWords = 30 });
            }

            var item = await _context.WritingPrompts.FindAsync(id.Value);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(WritingPrompt model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == 0)
            {
                _context.WritingPrompts.Add(model);
                TempData["success"] = "Đã tạo đề viết.";
            }
            else
            {
                _context.WritingPrompts.Update(model);
                TempData["success"] = "Đã cập nhật đề viết.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.WritingPrompts.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đề viết." });
            }

            _context.WritingPrompts.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa đề viết." });
        }
    }
}
