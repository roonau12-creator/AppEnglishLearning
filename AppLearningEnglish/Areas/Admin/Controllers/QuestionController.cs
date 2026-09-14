using AppLearningEnglish.Business.Services.IServices;
using AppLearningEnglish.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppLearningEnglish.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService
            _questionService;

        private readonly IExerciseService
            _exerciseService;

        public QuestionController(
            IQuestionService questionService,
            IExerciseService exerciseService)
        {
            _questionService =
                questionService;

            _exerciseService =
                exerciseService;
        }

        // ==========================================
        // INDEX
        // ==========================================

        public async Task<IActionResult> Index(
            int? exerciseId)
        {
            await LoadExercises();

            if (exerciseId.HasValue &&
                exerciseId.Value > 0)
            {
                ViewBag.SelectedExerciseId =
                    exerciseId.Value;
            }

            return View();
        }

        // ==========================================
        // GET ALL
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            int? exerciseId)
        {
            var questions =
                await _questionService
                    .SearchQuestionAsync(
                        search,
                        exerciseId);

            return Json(new
            {
                data = questions
            });
        }

        // ==========================================
        // UPSERT GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Upsert(
            int? id,
            int? exerciseId)
        {
            await LoadExercises();

            // CREATE
            if (id == null || id == 0)
            {
                var question =
                    new Question();

                if (exerciseId.HasValue &&
                    exerciseId.Value > 0)
                {
                    question.ExerciseId =
                        exerciseId.Value;
                }

                return View(question);
            }

            // EDIT
            var existingQuestion =
                await _questionService
                    .GetQuestionByIdAsync(
                        id.Value);

            if (existingQuestion == null)
                return NotFound();

            return View(existingQuestion);
        }

        // ==========================================
        // UPSERT POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(
            Question question)
        {
            if (!ModelState.IsValid)
            {
                await LoadExercises();

                return View(question);
            }

            bool questionExists =
                await _questionService
                    .IsQuestionExistsAsync(
                        question.ExerciseId,
                        question.QuestionText,
                        question.Id);

            if (questionExists)
            {
                ModelState.AddModelError(
                    nameof(question.QuestionText),
                    "Question này đã tồn tại trong Exercise.");

                await LoadExercises();

                return View(question);
            }

            if (question.Id == 0)
            {
                await _questionService
                    .CreateQuestionAsync(
                        question);

                TempData["success"] =
                    "Thêm Question thành công.";
            }
            else
            {
                await _questionService
                    .UpdateQuestionAsync(
                        question);

                TempData["success"] =
                    "Cập nhật Question thành công.";
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
            var question =
                await _questionService
                    .GetQuestionByIdAsync(id);

            if (question == null)
                return NotFound();

            return View(question);
        }

        // ==========================================
        // DELETE
        // ==========================================

        [HttpDelete]
        public async Task<IActionResult> Delete(
            int id)
        {
            bool result =
                await _questionService
                    .DeleteQuestionAsync(id);

            if (!result)
            {
                return Json(new
                {
                    success = false,
                    message =
                        "Không tìm thấy Question."
                });
            }

            return Json(new
            {
                success = true,
                message =
                    "Xóa Question thành công."
            });
        }

        // ==========================================
        // LOAD EXERCISES
        // ==========================================

        private async Task LoadExercises()
        {
            var exercises =
                await _exerciseService
                    .GetAllExerciseAsync();

            ViewBag.Exercises =
                new SelectList(
                    exercises,
                    "Id",
                    "Question");
        }
    }
}