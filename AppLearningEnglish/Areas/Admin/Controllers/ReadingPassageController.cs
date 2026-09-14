using AppLearningEnglish.DataAccess.Data;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReadingPassageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReadingPassageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.ReadingPassages
                .AsNoTracking()
                .Include(x => x.Questions)
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
                return View(new ReadingPassage { Level = "Beginner" });
            }

            var item = await _context.ReadingPassages.FindAsync(id.Value);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ReadingPassage model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == 0)
            {
                _context.ReadingPassages.Add(model);
                TempData["success"] = "Đã tạo bài đọc.";
            }
            else
            {
                var existing = await _context.ReadingPassages.FindAsync(model.Id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Title = model.Title;
                existing.Level = model.Level;
                existing.Topic = model.Topic;
                existing.Body = model.Body;
                TempData["success"] = "Đã cập nhật bài đọc.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.ReadingPassages
                .Include(x => x.Questions)
                .FirstOrDefaultAsync(x => x.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(ReadingQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.Prompt) || string.IsNullOrWhiteSpace(question.Answer))
            {
                TempData["error"] = "Cần câu hỏi và đáp án.";
                return RedirectToAction(nameof(Details), new { id = question.ReadingPassageId });
            }

            _context.ReadingQuestions.Add(question);
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã thêm câu hỏi.";
            return RedirectToAction(nameof(Details), new { id = question.ReadingPassageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.ReadingQuestions.FindAsync(id);
            if (question == null)
            {
                return Json(new { success = false, message = "Không tìm thấy câu hỏi." });
            }

            _context.ReadingQuestions.Remove(question);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa câu hỏi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ReadingPassages.FindAsync(id);
            if (item == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài đọc." });
            }

            _context.ReadingPassages.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa bài đọc." });
        }
    }
}
