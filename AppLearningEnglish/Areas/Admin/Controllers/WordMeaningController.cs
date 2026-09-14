using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AppLearningEnglish.Business.Services.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppLearningEnglish.Models;
using Microsoft.EntityFrameworkCore;
namespace AppLearningEnglish.Areas.Admin.Controllers
{
     [Area("Admin")]
    public class WordMeaningController : Controller
    {
        private readonly IWordMeaningService
            _wordMeaningService;

        private readonly IWordService
            _wordService;

        public WordMeaningController(
            IWordMeaningService wordMeaningService,
            IWordService wordService)
        {
            _wordMeaningService =
                wordMeaningService;

            _wordService =
                wordService;
        }

        // =====================================
        // INDEX
        // =====================================

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

        // =====================================
        // GET ALL
        // =====================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int? wordId,
            string? language)
        {
            var meanings =
                await _wordMeaningService
                    .SearchWordMeaningAsync(
                        search,
                        wordId,
                        language);

            return Json(new
            {
                data = meanings
            });
        }

        // =====================================
        // UPSERT GET
        // =====================================

        [HttpGet]
        public async Task<IActionResult> Upsert(
            int? id,
            int? wordId)
        {
            await LoadWords();

            // CREATE
            if (id == null || id == 0)
            {
                var wordMeaning =
                    new WordMeaning();

                if (wordId.HasValue &&
                    wordId.Value > 0)
                {
                    wordMeaning.WordId =
                        wordId.Value;
                }

                return View(wordMeaning);
            }

            // EDIT
            var existingWordMeaning =
                await _wordMeaningService
                    .GetWordMeaningByIdAsync(
                        id.Value);

            if (existingWordMeaning == null)
                return NotFound();

            return View(existingWordMeaning);
        }

        // =====================================
        // UPSERT POST
        // =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            WordMeaning wordMeaning)
        {
            if (!ModelState.IsValid)
            {
                await LoadWords();

                return View(wordMeaning);
            }

            bool exists =
                await _wordMeaningService
                    .IsMeaningExistsAsync(
                        wordMeaning.WordId,
                        wordMeaning.Language,
                        wordMeaning.Meaning,
                        wordMeaning.Id);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(wordMeaning.Meaning),
                    "Meaning này đã tồn tại cho Word và Language này.");

                await LoadWords();

                return View(wordMeaning);
            }

            if (wordMeaning.Id == 0)
            {
                await _wordMeaningService
                    .CreateWordMeaningAsync(
                        wordMeaning);

                TempData["success"] =
                    "Thêm Meaning thành công.";
            }
            else
            {
                await _wordMeaningService
                    .UpdateWordMeaningAsync(
                        wordMeaning);

                TempData["success"] =
                    "Cập nhật Meaning thành công.";
            }

            return RedirectToAction(
                nameof(Index));
        }

        // =====================================
        // DETAILS
        // =====================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var wordMeaning =
                await _wordMeaningService
                    .GetWordMeaningByIdAsync(id);

            if (wordMeaning == null)
                return NotFound();

            return View(wordMeaning);
        }

        // =====================================
        // DELETE
        // =====================================

        [HttpDelete]
        public async Task<IActionResult> Delete(
            int id)
        {
            bool result =
                await _wordMeaningService
                    .DeleteWordMeaningAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không tìm thấy Meaning."
                });
            }

            return Json(new
            {
                success = true,
                message =
                    "Xóa Meaning thành công."
            });
        }

        // =====================================
        // LOAD WORD DROPDOWN
        // =====================================

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