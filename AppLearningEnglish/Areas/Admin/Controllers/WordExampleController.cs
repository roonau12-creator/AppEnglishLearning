using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AppLearningEnglish.Business.Services.IServices;
using Microsoft.EntityFrameworkCore;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WordExampleController : Controller
    {
        private readonly IWordExampleService
            _wordExampleService;

        private readonly IWordService
            _wordService;

        public WordExampleController(
            IWordExampleService wordExampleService,
            IWordService wordService)
        {
            _wordExampleService =
                wordExampleService;

            _wordService =
                wordService;
        }

        // ==========================================
        // INDEX
        // ==========================================

        public async Task<IActionResult> Index(
            int? wordId)
        {
            await LoadWords();

            if (wordId.HasValue &&
                wordId.Value > 0)
            {
                ViewBag.SelectedWordId =
                    wordId.Value;
            }

            return View();
        }

        // ==========================================
        // GET ALL
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int? wordId)
        {
            var examples =
                await _wordExampleService
                    .SearchWordExampleAsync(
                        search,
                        wordId);

            return Json(new
            {
                data = examples
            });
        }

        // ==========================================
        // UPSERT GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Upsert(
            int? id,
            int? wordId)
        {
            await LoadWords();

            // CREATE
            if (id == null || id == 0)
            {
                var example =
                    new WordExample();

                if (wordId.HasValue &&
                    wordId.Value > 0)
                {
                    example.WordId =
                        wordId.Value;
                }

                return View(example);
            }

            // EDIT
            var existingExample =
                await _wordExampleService
                    .GetWordExampleByIdAsync(
                        id.Value);

            if (existingExample == null)
                return NotFound();

            return View(existingExample);
        }

        // ==========================================
        // UPSERT POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            WordExample wordExample)
        {
            if (!ModelState.IsValid)
            {
                await LoadWords();

                return View(wordExample);
            }

            bool exists =
                await _wordExampleService
                    .IsExampleExistsAsync(
                        wordExample.WordId,
                        wordExample.EnglishSentence,
                        wordExample.Id);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(wordExample.EnglishSentence),
                    "Câu ví dụ này đã tồn tại cho Word.");

                await LoadWords();

                return View(wordExample);
            }

            if (wordExample.Id == 0)
            {
                await _wordExampleService
                    .CreateWordExampleAsync(
                        wordExample);

                TempData["success"] =
                    "Thêm câu ví dụ thành công.";
            }
            else
            {
                await _wordExampleService
                    .UpdateWordExampleAsync(
                        wordExample);

                TempData["success"] =
                    "Cập nhật câu ví dụ thành công.";
            }

            return RedirectToAction(
                nameof(Index));
        }

        // ==========================================
        // DETAILS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var example =
                await _wordExampleService
                    .GetWordExampleByIdAsync(id);

            if (example == null)
                return NotFound();

            return View(example);
        }

        // ==========================================
        // DELETE
        // ==========================================

        [HttpDelete]
        public async Task<IActionResult> Delete(
            int id)
        {
            bool result =
                await _wordExampleService
                    .DeleteWordExampleAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không tìm thấy câu ví dụ."
                });
            }

            return Json(new
            {
                success = true,
                message =
                    "Xóa câu ví dụ thành công."
            });
        }

        // ==========================================
        // LOAD WORDS
        // ==========================================

        private async Task LoadWords()
        {
            var words =
                await _wordService
                    .GetAllWordAsync();

            ViewBag.Words =
                new SelectList(
                    words,
                    "Id",
                    "WordText");
        }
    }
}