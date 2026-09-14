using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WordController : Controller
    {
        private readonly IWordService _wordService;
        private readonly ILessonService _lessonService;

        public WordController(
            IWordService wordService,
            ILessonService lessonService)
        {
            _wordService = wordService;
            _lessonService = lessonService;
        }

        // =========================
        // INDEX
        // =========================

        public IActionResult Index()
        {
            return View();
        }

        // =========================
        // GET ALL
        // =========================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            string? partOfSpeech)
        {
            var words =
                await _wordService.SearchWordAsync(
                    search,
                    partOfSpeech);

            return Json(new
            {
                data = words
            });
        }

        // =========================
        // UPSERT GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            await LoadLessons();

            if (id == null || id == 0)
            {
                return View(new Word());
            }

            var word =
                await _wordService
                    .GetWordByIdAsync(id.Value);

            if (word == null)
                return NotFound();

            return View(word);
        }

        // =========================
        // UPSERT POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            Word word)
        {
            if (!ModelState.IsValid)
            {
                await LoadLessons();
                return View(word);
            }

            bool exists =
                await _wordService.IsWordExistsAsync(
                    word.WordText,
                    word.Id);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(word.WordText),
                    "Từ vựng này đã tồn tại.");

                await LoadLessons();
                return View(word);
            }

            if (word.Id == 0)
            {
                await _wordService
                    .CreateWordAsync(word);

                TempData["success"] =
                    "Thêm từ vựng thành công.";
            }
            else
            {
                await _wordService
                    .UpdateWordAsync(word);

                TempData["success"] =
                    "Cập nhật từ vựng thành công.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DETAILS
        // =========================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var word =
                await _wordService
                    .GetWordByIdAsync(id);

            if (word == null)
                return NotFound();

            return View(word);
        }

        // =========================
        // DELETE
        // =========================

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            bool result =
                await _wordService
                    .DeleteWordAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy từ vựng."
                });
            }

            return Json(new
            {
                success = true,
                message = "Xóa từ vựng thành công."
            });
        }

        private async Task LoadLessons()
        {
            var lessons = await _lessonService.GetAllLessonAsync();

            ViewBag.Lessons = new SelectList(
                lessons.Select(x => new
                {
                    x.Id,
                    Name = $"{x.Title} ({x.Course?.Name})"
                }),
                "Id",
                "Name");
        }
    }
}